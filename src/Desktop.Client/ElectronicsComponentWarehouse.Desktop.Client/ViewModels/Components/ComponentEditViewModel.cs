// ComponentEditViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ElectronicsComponentWarehouse.Desktop.Client.Models.Components;
using ElectronicsComponentWarehouse.Desktop.Client.Services;
using ElectronicsComponentWarehouse.Desktop.Client.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace ElectronicsComponentWarehouse.Desktop.Client.ViewModels.Components
{
    /// <summary>
    /// ViewModel для редактирования компонента
    /// </summary>
    public partial class ComponentEditViewModel : ObservableObject
    {
        private readonly IComponentService _componentService;
        private readonly ICategoryService _categoryService;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private ComponentModel _component = new();

        [ObservableProperty]
        private ObservableCollection<CategoryItemViewModel> _categories = new();

        [ObservableProperty]
        private bool _isEditMode;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _title = "Новый компонент";

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _hasChanges;

        private ComponentModel? _originalComponent;

        public ComponentEditViewModel(
            IComponentService componentService,
            ICategoryService categoryService,
            IDialogService dialogService)
        {
            _componentService = componentService;
            _categoryService = categoryService;
            _dialogService = dialogService;

            InitializeAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Инициализация ViewModel
        /// </summary>
        public async Task InitializeAsync(ComponentModel? component = null)
        {
            await LoadCategoriesAsync();

            if (component != null)
            {
                // Режим редактирования
                _originalComponent = component;
                Component = component.CreateEditableCopy();
                IsEditMode = true;
                Title = $"Редактирование: {component.Name}";
            }
            else
            {
                // Режим создания
                Component = new ComponentModel
                {
                    StockQuantity = 0,
                    MinimumStockLevel = 10,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };
                IsEditMode = false;
                Title = "Новый компонент";
            }

            HasChanges = false;
            ErrorMessage = string.Empty;
        }

        /// <summary>
        /// Команда сохранения компонента
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task SaveAsync()
        {
            if (!ValidateComponent())
                return;

            await ExecuteAsync(async () =>
            {
                ComponentModel? result;

                if (IsEditMode)
                {
                    // Обновление существующего компонента
                    result = await _componentService.UpdateComponentAsync(Component);
                }
                else
                {
                    // Создание нового компонента
                    result = await _componentService.CreateComponentAsync(Component);
                }

                if (result != null)
                {
                    // Отправляем сообщение об успешном сохранении
                    WeakReferenceMessenger.Default.Send(new ComponentSavedMessage(result, IsEditMode));

                    // Закрываем окно
                    CloseWindow();
                }
                else
                {
                    ErrorMessage = "Не удалось сохранить компонент";
                }
            }, "Сохранение компонента");
        }

        private bool CanSave() => !IsBusy && HasChanges;

        /// <summary>
        /// Команда отмены
        /// </summary>
        [RelayCommand]
        private void Cancel()
        {
            if (HasChanges)
            {
                var confirm = MessageBox.Show(
                    "У вас есть несохраненные изменения. Вы уверены, что хотите отменить?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirm != MessageBoxResult.Yes)
                    return;
            }

            CloseWindow();
        }

        /// <summary>
        /// Команда проверки данных
        /// </summary>
        [RelayCommand]
        private void Validate()
        {
            if (ValidateComponent())
            {
                _dialogService.ShowMessageAsync("Проверка",
                    "Все данные корректны").ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Валидация компонента
        /// </summary>
        private bool ValidateComponent()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Component.Name))
            {
                ErrorMessage = "Название компонента обязательно";
                return false;
            }

            if (Component.Name.Length < 2 || Component.Name.Length > 200)
            {
                ErrorMessage = "Название должно содержать от 2 до 200 символов";
                return false;
            }

            if (Component.StockQuantity < 0)
            {
                ErrorMessage = "Количество не может быть отрицательным";
                return false;
            }

            if (Component.MinimumStockLevel < 0)
            {
                ErrorMessage = "Минимальный запас не может быть отрицательным";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Component.StorageCellNumber))
            {
                ErrorMessage = "Номер ячейки хранения обязателен";
                return false;
            }

            if (Component.CategoryId <= 0)
            {
                ErrorMessage = "Необходимо выбрать категорию";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Загрузка категорий
        /// </summary>
        private async Task LoadCategoriesAsync()
        {
            await ExecuteAsync(async () =>
            {
                var categories = await _categoryService.GetAllCategoriesAsync();

                Categories.Clear();
                foreach (var category in categories)
                {
                    Categories.Add(new CategoryItemViewModel
                    {
                        Id = category.Id,
                        Name = category.Name,
                        FullPath = category.Name // TODO: Реализовать полный путь
                    });
                }

                if (Categories.Count > 0 && Component.CategoryId <= 0)
                {
                    Component.CategoryId = Categories[0].Id;
                }
            }, "Загрузка категорий");
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
                await action();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowMessageAsync("Ошибка",
                    $"{operationName} не удалось: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Закрытие окна
        /// </summary>
        private void CloseWindow()
        {
            WeakReferenceMessenger.Default.Send(new CloseWindowMessage());
        }

        partial void OnComponentChanged(ComponentModel value)
        {
            HasChanges = true;
        }
    }

    /// <summary>
    /// ViewModel для элемента категории
    /// </summary>
    public class CategoryItemViewModel : ObservableObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;

        public override string ToString() => FullPath;
    }

    /// <summary>
    /// Сообщение о сохранении компонента
    /// </summary>
    public class ComponentSavedMessage
    {
        public ComponentModel Component { get; }
        public bool WasEdit { get; }

        public ComponentSavedMessage(ComponentModel component, bool wasEdit)
        {
            Component = component;
            WasEdit = wasEdit;
        }
    }

    /// <summary>
    /// Сообщение о закрытии окна
    /// </summary>
    public class CloseWindowMessage
    {
    }
}