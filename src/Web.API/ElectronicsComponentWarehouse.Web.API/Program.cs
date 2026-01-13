using ElectronicsComponentWarehouse.Application;
using ElectronicsComponentWarehouse.Infrastructure.Data;
using ElectronicsComponentWarehouse.Web.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Настройка Serilog для логирования
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    Log.Information("Starting web application");

    // Добавление Serilog в качестве провайдера логирования
    builder.Host.UseSerilog();

    // Добавление сервисов в контейнер
    ConfigureServices(builder.Services, builder.Configuration);

    var app = builder.Build();

    // Настройка конвейера HTTP запросов
    ConfigurePipeline(app);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Метод для настройки сервисов
void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // Контроллеры
    services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        });

    // Swagger/OpenAPI
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = configuration["ApiSettings:ApiVersion"] ?? "1.0",
            Title = configuration["ApiSettings:ApiTitle"] ?? "Electronics Component Warehouse API",
            Description = configuration["ApiSettings:ApiDescription"] ?? "API для управления складом электронных компонентов",
            Contact = new OpenApiContact
            {
                Name = configuration["ApiSettings:ContactName"] ?? "Администрация склада",
                Email = configuration["ApiSettings:ContactEmail"] ?? "admin@warehouse.local"
            }
        });

        // Добавление JWT аутентификации в Swagger
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = @"JWT Authorization header using the Bearer scheme. 
                          Enter 'Bearer' [space] and then your token in the text input below.
                          Example: 'Bearer 12345abcdef'",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header
                },
                new List<string>()
            }
        });

        // Группировка по тегам (контроллерам)
        options.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
        options.DocInclusionPredicate((name, api) => true);

        // Включение комментариев XML
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

    // CORS
    services.AddCors(options =>
    {
        options.AddPolicy("AllowClient",
            policy =>
            {
                var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" };
                var allowedMethods = configuration.GetSection("Cors:AllowedMethods").Get<string[]>() ?? new[] { "GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS" };
                var allowedHeaders = configuration.GetSection("Cors:AllowedHeaders").Get<string[]>() ?? new[] { "*" };

                policy.WithOrigins(allowedOrigins)
                      .WithMethods(allowedMethods)
                      .WithHeaders(allowedHeaders)
                      .AllowCredentials();
            });
    });

    // JWT аутентификация
    var jwtSettings = configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT secret is not configured");

    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Warning("Authentication failed: {ErrorMessage}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Log.Debug("Token validated for user: {UserName}", context.Principal?.Identity?.Name);
                return Task.CompletedTask;
            }
        };
    });

    // Авторизация
    services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
    });

    // Регистрация слоев приложения
    services.AddApplication();
    services.AddInfrastructureData(configuration);

    // Health checks
    services.AddHealthChecks();
}

// Метод для настройки конвейера HTTP
void ConfigurePipeline(WebApplication app)
{
    // Настройка конвейера в зависимости от окружения
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Warehouse API v1");
            options.RoutePrefix = "swagger";
            options.DisplayRequestDuration();
            options.EnableTryItOutByDefault();
        });
    }
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    // Middleware
    app.UseMiddleware<RequestLoggingMiddleware>();
    app.UseMiddleware<ModelValidationMiddleware>();
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // Стандартные middleware
    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseCors("AllowClient");
    app.UseAuthentication();
    app.UseAuthorization();

    // Endpoints
    app.MapControllers();
    app.MapHealthChecks("/health");

    // Миграция базы данных при запуске (только в Development)
    if (app.Environment.IsDevelopment())
    {
        using (var scope = app.Services.CreateScope())
        {
            try
            {
                var serviceProvider = scope.ServiceProvider;
                serviceProvider.MigrateDatabaseAsync().Wait();
                Log.Information("Database migration completed successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while migrating the database");
                throw;
            }
        }
    }
}