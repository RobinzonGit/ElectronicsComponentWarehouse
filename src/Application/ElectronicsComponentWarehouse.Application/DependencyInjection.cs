//Создаем класс для регистрации сервисов в DI-контейнере
using ElectronicsComponentWarehouse.Application.Mappings;
using ElectronicsComponentWarehouse.Application.Services.Implementations;
using ElectronicsComponentWarehouse.Application.Services.Interfaces;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ElectronicsComponentWarehouse.Application
{
    /// <summary>
    /// Класс для регистрации зависимостей слоя Application
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Добавляет сервисы слоя Application в DI-контейнер
        /// </summary>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Регистрируем AutoMapper
            services.AddAutoMapper(typeof(MappingProfile).Assembly);
            
            // Регистрируем FluentValidation
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
            
            // Регистрируем сервисы
            services.AddScoped<IComponentService, ComponentService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            
            return services;
        }
    }
}
