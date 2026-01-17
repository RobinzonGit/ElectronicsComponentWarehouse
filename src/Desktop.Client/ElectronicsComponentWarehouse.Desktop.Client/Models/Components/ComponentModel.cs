using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ElectronicsComponentWarehouse.Desktop.Client.Models.Components
{
    /// <summary>
    /// Модель компонента для клиентского приложения
    /// </summary>
    public partial class ComponentModel : ObservableObject
    {
        [ObservableProperty]
        private int _id;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasChanges))]
        [NotifyPropertyChangedFor(nameof(DisplayName))]
        private string _name = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasChanges))]
        private string? _description;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasChanges))]
        [NotifyPropertyChangedFor(nameof(IsLowStock))]
        [NotifyPropertyChangedFor(nameof(StockStatus))]
        [NotifyPropertyChangedFor(nameof(TotalValue))]
        private int _stockQuantity;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasChanges))]
        private string _storageCellNumber = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasChanges))]
        private string? _manufacturer;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasChanges))]
        private string? _modelNumber;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasChanges))]
        private string? _datasheetLink;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasChanges))]
        [NotifyPropertyChangedFor(nameof(IsLowStock))]
        [NotifyPropertyChangedFor(nameof(StockStatus))]
        private int _minimumStockLevel = 10;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasChanges))]
        [NotifyPropertyChangedFor(nameof(TotalValue))]
        [NotifyPropertyChangedFor(nameof(FormattedUnitPrice))]
        private decimal? _unitPrice;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasChanges))]
        private int _categoryId;

        [ObservableProperty]
        private string? _categoryName;

        [ObservableProperty]
        private DateTime _createdAt;

        [ObservableProperty]
        private DateTime _lastUpdated;

        // Вычисляемые свойства
        public bool IsLowStock => StockQuantity <= MinimumStockLevel;

        public string StockStatus => IsLowStock ? "Низкий запас" : "Достаточно";

        public decimal? TotalValue => UnitPrice.HasValue ? UnitPrice.Value * StockQuantity : null;

        public string DisplayName => $"{Name} ({StockQuantity} шт.)";

        public string FormattedUnitPrice => UnitPrice?.ToString("C2") ?? "Не указана";

        public string FormattedTotalValue => TotalValue?.ToString("C2") ?? "Не рассчитана";

        // Для отслеживания изменений
        private ComponentModel? _originalState;

        public bool HasChanges
        {
            get
            {
                if (_originalState == null) return false;

                return Name != _originalState.Name ||
                       Description != _originalState.Description ||
                       StockQuantity != _originalState.StockQuantity ||
                       StorageCellNumber != _originalState.StorageCellNumber ||
                       Manufacturer != _originalState.Manufacturer ||
                       ModelNumber != _originalState.ModelNumber ||
                       DatasheetLink != _originalState.DatasheetLink ||
                       MinimumStockLevel != _originalState.MinimumStockLevel ||
                       UnitPrice != _originalState.UnitPrice ||
                       CategoryId != _originalState.CategoryId;
            }
        }

        /// <summary>
        //Сохранить текущее состояние как оригинальное
        /// </summary>
        public void SaveState()
        {
            _originalState = MemberwiseClone() as ComponentModel;
        }

        /// <summary>
        //Восстановить оригинальное состояние
        /// </summary>
        public void RestoreState()
        {
            if (_originalState == null) return;

            Name = _originalState.Name;
            Description = _originalState.Description;
            StockQuantity = _originalState.StockQuantity;
            StorageCellNumber = _originalState.StorageCellNumber;
            Manufacturer = _originalState.Manufacturer;
            ModelNumber = _originalState.ModelNumber;
            DatasheetLink = _originalState.DatasheetLink;
            MinimumStockLevel = _originalState.MinimumStockLevel;
            UnitPrice = _originalState.UnitPrice;
            CategoryId = _originalState.CategoryId;
        }

        /// <summary>
        //Создать копию для редактирования
        /// </summary>
        public ComponentModel CreateEditableCopy()
        {
            var copy = MemberwiseClone() as ComponentModel;
            copy?.SaveState();
            return copy!;
        }
    }
}