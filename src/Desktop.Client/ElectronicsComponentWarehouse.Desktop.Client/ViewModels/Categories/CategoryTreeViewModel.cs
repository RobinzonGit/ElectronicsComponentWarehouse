// CategoryTreeViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ElectronicsComponentWarehouse.Desktop.Client.Models.Categories;
using ElectronicsComponentWarehouse.Desktop.Client.Services;
using ElectronicsComponentWarehouse.Desktop.Client.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ElectronicsComponentWarehouse.Desktop.Client.ViewModels.Categories
{
    /// <summary>
    /// ViewModel для управления деревом категорий
    /// </summary>
    public partial class CategoryTreeViewModel : ObservableObject
    {
        private readonly ICategoryService _categoryService;
        private readonly CurrentUserService _currentUserService;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private ObservableCollection<CategoryModel> _categories = new();

        [ObservableProperty]
        private CategoryModel? _selectedCategory;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        public CategoryTreeViewModel(
            ICategoryService categoryService,
            CurrentUserService currentUserService,
            IDialogService dialogService)
        {
            _categoryService = categoryService;
            _currentUserService = currentUserService;
            _dialogService = dialogService;

            LoadCategoriesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Команда загрузки категорий
        /// </summary>
        [RelayCommand]
        private async Task LoadCategoriesAsync()
        {
            await ExecuteAsync(async () =>
            {
                var categories = await _categoryService.GetCategoryHierarchyAsync();

                Categories.Clear();
                foreach (var category in categories)
                {
                    Categories.Add(category);
                }

                StatusMessage = $"Загружено {GetTotalCategories(Categories)} категорий";
            }, "Загрузка категорий");
        }

        /// <summary>
        /// Команда добавления категории
        /// </summary>
        [RelayCommand]
        private void AddCategory()
        {
            if (!_currentUserService.IsAdmin)
            {
                _dialogService.ShowMessageAsync("Доступ запрещен",
                    "Только администраторы могут добавлять категории").ConfigureAwait(false);
                return;
            }

            WeakReferenceMessenger.Default.Send(new OpenCategoryEditMessage(null, SelectedCategory?.Id));
        }

        /// <summary>
        /// Команда редактирования категории
        /// </summary>
        [RelayCommand]
        private void EditCategory()
        {
            if (SelectedCategory == null) return;

            if (!_currentUserService.IsAdmin)
            {
                _dialogService.ShowMessageAsync("Доступ запрещен",
                    "Только администраторы могут редактировать категории").ConfigureAwait(false);
                return;
            }

            WeakReferenceMessenger.Default.Send(new OpenCategoryEditMessage(SelectedCategory, null));
        }

        /// <summary>
        /// Команда удаления категории
        /// </summary>
        [RelayCommand]
        private async Task DeleteCategoryAsync()
        {
            if (SelectedCategory == null) return;

            if (!_currentUserService.IsAdmin)
            {
                await _dialogService.ShowMessageAsync("Доступ запрещен",
                    "Только администраторы могут удалять категории");
                return;
            }

            // Проверяем возможность удаления
            var check = await _categoryService.CheckCategoryDeletionAsync(SelectedCategory.Id);

            if (!check.CanBeDeleted)
            {
                await _dialogService.ShowMessageAsync("Невозможно удалить", check.Message);
                return;
            }

            var confirm = await _dialogService.ShowConfirmationAsync(
                "Подтверждение удаления",
                $"Вы уверены, что хотите удалить категорию '{SelectedCategory.Name}'?" +
                $"\n\n{check.Message}");

            if (confirm)
            {
                await ExecuteAsync(async () =>
                {
                    var success = await _categoryService.DeleteCategoryAsync(SelectedCategory.Id);
                    if (success)
                    {
                        RemoveCategoryFromTree(Categories, SelectedCategory.Id);
                        SelectedCategory = null;
                        StatusMessage = "Категория удалена";
                    }
                    else
                    {
                        await _dialogService.ShowMessageAsync("Ошибка", "Не удалось удалить категорию");
                    }
                }, "Удаление категории");
            }
        }

        /// <summary>
        /// Команда обновления дерева
        /// </summary>
        [RelayCommand]
        public async Task RefreshAsync()
        {
            await LoadCategoriesAsync();
        }

        /// <summary>
        /// Команда разворачивания всех категорий
        /// </summary>
        [RelayCommand]
        private void ExpandAll()
        {
            SetExpandedState(Categories, true);
        }

        /// <summary>
        /// Команда сворачивания всех категорий
        /// </summary>
        [RelayCommand]
        private void CollapseAll()
        {
            SetExpandedState(Categories, false);
        }

        /// <summary>
        /// Получение общего количества категорий (включая вложенные)
        /// </summary>
        private int GetTotalCategories(ObservableCollection<CategoryModel> categories)
        {
            int count = categories.Count;

            foreach (var category in categories)
            {
                count += GetTotalCategories(category.ChildCategories);
            }

            return count;
        }

        /// <summary>
        /// Удаление категории из дерева
        /// </summary>
        private bool RemoveCategoryFromTree(ObservableCollection<CategoryModel> categories, int categoryId)
        {
            for (int i = 0; i < categories.Count; i++)
            {
                if (categories[i].Id == categoryId)
                {
                    categories.RemoveAt(i);
                    return true;
                }

                if (RemoveCategoryFromTree(categories[i].ChildCategories, categoryId))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Установка состояния развернутости для всех категорий
        /// </summary>
        private void SetExpandedState(ObservableCollection<CategoryModel> categories, bool isExpanded)
        {
            foreach (var category in categories)
            {
                category.IsExpanded = isExpanded;
                SetExpandedState(category.ChildCategories, isExpanded);
            }
        }

        /// <summary>
        /// Выполнение асинхронной операции
        /// </summary>
        private async Task ExecuteAsync(Func<Task> action, string operationName)
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                StatusMessage = $"{operationName}...";
                await action();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowMessageAsync("Ошибка",
                    $"{operationName} не удалось: {ex.Message}");
                StatusMessage = "Ошибка при выполнении операции";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    /// <summary>
    /// Сообщение для открытия редактора категории
    /// </summary>
    public class OpenCategoryEditMessage
    {
        public CategoryModel? Category { get; }
        public int? ParentCategoryId { get; }

        public OpenCategoryEditMessage(CategoryModel? category, int? parentCategoryId)
        {
            Category = category;
            ParentCategoryId = parentCategoryId;
        }
    }
}