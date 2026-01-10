//Валидатор для регистрации пользователя
using ElectronicsComponentWarehouse.Application.DTOs.Users;
using FluentValidation;

namespace ElectronicsComponentWarehouse.Application.Validators.Users
{
    /// <summary>
    /// Валидатор для DTO создания пользователя
    /// </summary>
    public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserDtoValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Имя пользователя обязательно")
                .Length(3, 50).WithMessage("Имя пользователя должно содержать от 3 до 50 символов")
                .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("Имя пользователя может содержать только буквы, цифры и подчеркивания");
            
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Пароль обязателен")
                .Length(6, 100).WithMessage("Пароль должен содержать от 6 до 100 символов")
                .Matches(@"[A-Z]").WithMessage("Пароль должен содержать хотя бы одну заглавную букву")
                .Matches(@"[a-z]").WithMessage("Пароль должен содержать хотя бы одну строчную букву")
                .Matches(@"[0-9]").WithMessage("Пароль должен содержать хотя бы одну цифру")
                .Matches(@"[\W_]").WithMessage("Пароль должен содержать хотя бы один специальный символ");
            
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email обязателен")
                .EmailAddress().WithMessage("Неверный формат email")
                .MaximumLength(100).WithMessage("Email не должен превышать 100 символов");
            
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Полное имя обязательно")
                .Length(2, 100).WithMessage("Полное имя должно содержать от 2 до 100 символов");
            
            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Роль обязательна")
                .Must(BeAValidRole).WithMessage("Неверная роль. Допустимые значения: User, Admin");
        }
        
        private bool BeAValidRole(string role)
        {
            return role.Equals("User", StringComparison.OrdinalIgnoreCase) 
                || role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }
    }
}
