//Валидатор для создания категории
using ElectronicsComponentWarehouse.Application.DTOs.Categories;
using FluentValidation;

namespace ElectronicsComponentWarehouse.Application.Validators.Categories
{
    /// <summary>
    /// Валидатор для DTO создания категории
    /// </summary>
    public class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
    {
        public CreateCategoryDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Название категории обязательно")
                .Length(2, 100).WithMessage("Название должно содержать от 2 до 100 символов")
                .Matches(@"^[A-Za-zА-Яа-я0-9\s\-_]+$").WithMessage("Название может содержать только буквы, цифры, пробелы, дефисы и подчеркивания");
            
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Описание не должно превышать 500 символов")
                .When(x => !string.IsNullOrEmpty(x.Description));
            
            RuleFor(x => x.ParentCategoryId)
                .GreaterThan(0).WithMessage("ID родительской категории должен быть положительным числом")
                .When(x => x.ParentCategoryId.HasValue);
        }
    }
}
