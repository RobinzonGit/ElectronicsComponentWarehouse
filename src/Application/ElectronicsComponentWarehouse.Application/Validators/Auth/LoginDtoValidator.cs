//Валидатор для входа в систему
using ElectronicsComponentWarehouse.Application.DTOs.Auth;
using FluentValidation;

namespace ElectronicsComponentWarehouse.Application.Validators.Auth
{
    /// <summary>
    /// Валидатор для DTO входа в систему
    /// </summary>
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Имя пользователя обязательно")
                .Length(3, 50).WithMessage("Имя пользователя должно содержать от 3 до 50 символов");
            
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Пароль обязателен")
                .Length(6, 100).WithMessage("Пароль должен содержать от 6 до 100 символов");
        }
    }
}
