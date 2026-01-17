using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace ElectronicsComponentWarehouse.Desktop.Client.Models.Categories
{
    /// <summary>
    /// Модель категории для клиентского приложения
    /// </summary>
    public partial class CategoryModel : ObservableObject
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
        private int? _parentCategoryId;

        [ObservableProperty]
        private string? _parentCategoryName;

        [ObservableProperty]
        private int _componentCount;

        [ObservableProperty]
        private int _childCategoryCount;

        [ObservableProperty]
        private DateTime _createdAt;

        [ObservableProperty]
        private DateTime _updatedAt;

        [ObservableProperty]
        private ObservableCollection<CategoryModel> _childCategories = new();

        // Для иерархического отображения
        [ObservableProperty]
        private bool _isExpanded;

        [ObservableProperty]
        private bool _isSelected;

        // Вычисляемые свойства
        public string DisplayName => $"{Name} ({ComponentCount})";

        public bool HasChildren => ChildCategories.Count > 0;

        // Для отслеживания изменений
        private CategoryModel? _originalState;

        public bool HasChanges
        {
            get
            {
                if (_originalState == null) return false;

                return Name != _originalState.Name ||
                       Description != _originalState.Description ||
                       ParentCategoryId != _originalState.ParentCategoryId;
            }
        }

        /// <summary>
        //Сохранить текущее состояние как оригинальное
        /// </summary>
        public void SaveState()
        {
            _originalState = MemberwiseClone() as CategoryModel;
        }

        /// <summary>
        //Восстановить оригинальное состояние
        /// </summary>
        public void RestoreState()
        {
            if (_originalState == null) return;

            Name = _originalState.Name;
            Description = _originalState.Description;
            ParentCategoryId = _originalState.ParentCategoryId;
        }

        /// <summary>
        //Добавить дочернюю категорию
        /// </summary>
        public void AddChild(CategoryModel child)
        {
            ChildCategories.Add(child);
            OnPropertyChanged(nameof(HasChildren));
        }

        /// <summary>
        //Удалить дочернюю категорию
        /// </summary>
        public bool RemoveChild(CategoryModel child)
        {
            var result = ChildCategories.Remove(child);
            if (result)
            {
                OnPropertyChanged(nameof(HasChildren));
            }
            return result;
        }
    }
}