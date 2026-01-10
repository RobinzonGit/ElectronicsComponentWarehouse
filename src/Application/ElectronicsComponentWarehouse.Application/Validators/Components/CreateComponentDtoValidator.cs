//Валидатор для создания компонента
using ElectronicsComponentWarehouse.Application.DTOs.Components;
using FluentValidation;

namespace ElectronicsComponentWarehouse.Application.Validators.Components
{
    /// <summary>
    /// Валидатор для DTO создания компонента
    /// </summary>
    public class CreateComponentDtoValidator : AbstractValidator<CreateComponentDto>
    {
        public CreateComponentDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Название компонента обязательно")
                .Length(2, 200).WithMessage("Название должно содержать от 2 до 200 символов");
            
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Описание не должно превышать 1000 символов")
                .When(x => !string.IsNullOrEmpty(x.Description));
            
            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Количество не может быть отрицательным");
            
            RuleFor(x => x.StorageCellNumber)
                .NotEmpty().WithMessage("Номер ячейки хранения обязателен")
                .Length(1, 50).WithMessage("Номер ячейки должен содержать от 1 до 50 символов")
                .Matches(@"^[A-Za-z0-9\-_]+$").WithMessage("Номер ячейки может содержать только буквы, цифры, дефисы и подчеркивания");
            
            RuleFor(x => x.Manufacturer)
                .MaximumLength(100).WithMessage("Производитель не должен превышать 100 символов")
                .When(x => !string.IsNullOrEmpty(x.Manufacturer));
            
            RuleFor(x => x.ModelNumber)
                .MaximumLength(100).WithMessage("Модель/парт-номер не должен превышать 100 символов")
                .When(x => !string.IsNullOrEmpty(x.ModelNumber));
            
            RuleFor(x => x.DatasheetLink)
                .MaximumLength(500).WithMessage("Ссылка на документацию не должна превышать 500 символов")
                .Must(BeAValidUrl).WithMessage("Неверный формат URL")
                .When(x => !string.IsNullOrEmpty(x.DatasheetLink));
            
            RuleFor(x => x.MinimumStockLevel)
                .GreaterThanOrEqualTo(0).WithMessage("Минимальный уровень запаса не может быть отрицательным");
            
            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Цена не может быть отрицательной")
                .When(x => x.UnitPrice.HasValue);
            
            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("ID категории должен быть положительным числом");
        }
        
        private bool BeAValidUrl(string? url)
        {
            if (string.IsNullOrEmpty(url))
                return true;
                
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) 
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
