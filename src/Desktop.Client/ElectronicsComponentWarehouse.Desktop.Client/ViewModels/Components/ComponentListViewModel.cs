// ComponentListViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ElectronicsComponentWarehouse.Desktop.Client.Models.Components;
using ElectronicsComponentWarehouse.Desktop.Client.Services;
using ElectronicsComponentWarehouse.Desktop.Client.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ElectronicsComponentWarehouse.Desktop.Client.ViewModels.Components
{
    /// <summary>
    /// ViewModel для управления списком компонентов
    /// </summary>
    public partial class ComponentListViewModel : ObservableObject
    {
        private readonly IComponentService _componentService;
        private readonly ICategoryService _categoryService;
        private readonly CurrentUserService _currentUserService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private ObservableCollection<ComponentModel> _components = new();

        [ObservableProperty]
        private ObservableCollection<ComponentModel> _filteredComponents = new();

        [ObservableProperty]
        private ComponentModel? _selectedComponent;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _searchStatus = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _showOnlyLowStock;

        [ObservableProperty]
        private int _selectedCategoryId;

        [ObservableProperty]
        private string _categoryFilter = "Все категории";

        private CollectionViewSource _componentsViewSource;

        public ComponentListViewModel(
            IComponentService componentService,
            ICategoryService categoryService,
            CurrentUserService currentUserService,
            IDialogService dialogService,
            INavigationService navigationService)
        {
            _componentService = componentService;
            _categoryService = categoryService;
            _currentUserService = currentUserService;
            _dialogService = dialogService;
            _navigationService = navigationService;

            _componentsViewSource = new CollectionViewSource { Source = Components };
            _componentsViewSource.Filter += OnComponentsFilter;

            // Загрузка данных при создании
            LoadDataAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Команда загрузки компонентов
        /// </summary>
        [RelayCommand]
        private async Task LoadComponentsAsync()
        {
            await ExecuteAsync(async () =>
            {
                var components = await _componentService.GetAllComponentsAsync();

                Components.Clear();
                foreach (var component in components)
                {
                    Components.Add(component);
                }

                SearchStatus = $"Загружено {Components.Count} компонентов";
            }, "Загрузка компонентов");
        }

        /// <summary>
        /// Команда поиска компонентов
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanSearch))]
        private async Task SearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadComponentsAsync();
                return;
            }

            await ExecuteAsync(async () =>
            {
                var foundComponents = await _componentService.SearchComponentsAsync(SearchText);

                Components.Clear();
                foreach (var component in foundComponents)
                {
                    Components.Add(component);
                }

                SearchStatus = $"Найдено {Components.Count} компонентов";
            }, "Поиск компонентов");
        }

        private bool CanSearch() => !IsBusy;

        /// <summary>
        /// Команда обновления списка
        /// </summary>
        [RelayCommand]
        public async Task RefreshAsync()
        {
            await LoadComponentsAsync();
        }

        /// <summary>
        /// Команда добавления компонента
        /// </summary>
        [RelayCommand]
        private void AddComponent()
        {
            if (!_currentUserService.IsAdmin)
            {
                _dialogService.ShowMessageAsync("Доступ запрещен",
                    "Только администраторы могут добавлять компоненты").ConfigureAwait(false);
                return;
            }

            WeakReferenceMessenger.Default.Send(new OpenComponentEditMessage(null));
        }

        /// <summary>
        /// Команда редактирования компонента
        /// </summary>
        [RelayCommand]
        private void EditComponent()
        {
            if (SelectedComponent == null) return;

            if (!_currentUserService.IsAdmin)
            {
                // Для обычных пользователей - быстрое редактирование
                WeakReferenceMessenger.Default.Send(new QuickEditComponentMessage(SelectedComponent));
                return;
            }

            WeakReferenceMessenger.Default.Send(new OpenComponentEditMessage(SelectedComponent));
        }

        /// <summary>
        /// Команда удаления компонента
        /// </summary>
        [RelayCommand]
        private async Task DeleteComponentAsync()
        {
            if (SelectedComponent == null) return;

            if (!_currentUserService.IsAdmin)
            {
                await _dialogService.ShowMessageAsync("Доступ запрещен",
                    "Только администраторы могут удалять компоненты");
                return;
            }

            var confirm = await _dialogService.ShowConfirmationAsync(
                "Подтверждение удаления",
                $"Вы уверены, что хотите удалить компонент '{SelectedComponent.Name}'?");

            if (confirm)
            {
                await ExecuteAsync(async () =>
                {
                    var success = await _componentService.DeleteComponentAsync(SelectedComponent.Id);
                    if (success)
                    {
                        Components.Remove(SelectedComponent);
                        SelectedComponent = null;
                        SearchStatus = "Компонент удален";
                    }
                    else
                    {
                        await _dialogService.ShowMessageAsync("Ошибка", "Не удалось удалить компонент");
                    }
                }, "Удаление компонента");
            }
        }

        /// <summary>
        /// Команда открытия даташита
        /// </summary>
        [RelayCommand]
        private void OpenDatasheet(string? datasheetLink)
        {
            if (string.IsNullOrEmpty(datasheetLink))
            {
                _dialogService.ShowMessageAsync("Информация", "Даташит не указан").ConfigureAwait(false);
                return;
            }

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = datasheetLink,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessageAsync("Ошибка",
                    $"Не удалось открыть даташит: {ex.Message}").ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Команда фильтрации по низкому запасу
        /// </summary>
        [RelayCommand]
        private void ToggleLowStockFilter()
        {
            ShowOnlyLowStock = !ShowOnlyLowStock;
            _componentsViewSource.View.Refresh();
            UpdateFilterStatus();
        }

        /// <summary>
        /// Команда экспорта списка
        /// </summary>
        [RelayCommand]
        private async Task ExportAsync()
        {
            await ExecuteAsync(async () =>
            {
                // TODO: Реализовать экспорт в CSV/Excel
                await _dialogService.ShowMessageAsync("Экспорт",
                    "Функция экспорта будет реализована в следующей версии");
            }, "Экспорт данных");
        }

        /// <summary>
        /// Фильтрация компонентов
        /// </summary>
        private void OnComponentsFilter(object sender, FilterEventArgs e)
        {
            if (e.Item is not ComponentModel component)
            {
                e.Accepted = false;
                return;
            }

            bool accept = true;

            // Фильтр по низкому запасу
            if (ShowOnlyLowStock)
            {
                accept = component.IsLowStock;
            }

            // Фильтр по категории
            if (SelectedCategoryId > 0)
            {
                accept = accept && component.CategoryId == SelectedCategoryId;
            }

            // Фильтр по поисковому запросу
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                accept = accept && (
                    component.Name.ToLower().Contains(searchLower) ||
                    component.Description?.ToLower().Contains(searchLower) == true ||
                    component.Manufacturer?.ToLower().Contains(searchLower) == true ||
                    component.ModelNumber?.ToLower().Contains(searchLower) == true);
            }

            e.Accepted = accept;
        }

        /// <summary>
        /// Обновление статуса фильтрации
        /// </summary>
        private void UpdateFilterStatus()
        {
            var total = Components.Count;
            var filtered = _componentsViewSource.View.Cast<ComponentModel>().Count();

            if (total == filtered)
            {
                SearchStatus = $"Всего: {total} компонентов";
            }
            else
            {
                SearchStatus = $"Показано: {filtered} из {total} компонентов";
            }
        }

        /// <summary>
        /// Загрузка начальных данных
        /// </summary>
        private async Task LoadDataAsync()
        {
            await LoadComponentsAsync();
        }

        /// <summary>
        /// Выполнение асинхронной операции с обработкой состояния
        /// </summary>
        private async Task ExecuteAsync(Func<Task> action, string operationName)
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                SearchStatus = $"{operationName}...";
                await action();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowMessageAsync("Ошибка",
                    $"{operationName} не удалось: {ex.Message}");
                SearchStatus = "Ошибка при выполнении операции";
            }
            finally
            {
                IsBusy = false;
                UpdateFilterStatus();
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            _componentsViewSource.View.Refresh();
            UpdateFilterStatus();
        }

        partial void OnShowOnlyLowStockChanged(bool value)
        {
            _componentsViewSource.View.Refresh();
            UpdateFilterStatus();
        }

        partial void OnSelectedCategoryIdChanged(int value)
        {
            _componentsViewSource.View.Refresh();
            UpdateFilterStatus();
        }
    }

    /// <summary>
    /// Сообщение для открытия редактора компонента
    /// </summary>
    public class OpenComponentEditMessage
    {
        public ComponentModel? Component { get; }

        public OpenComponentEditMessage(ComponentModel? component)
        {
            Component = component;
        }
    }

    /// <summary>
    /// Сообщение для быстрого редактирования компонента
    /// </summary>
    public class QuickEditComponentMessage
    {
        public ComponentModel Component { get; }

        public QuickEditComponentMessage(ComponentModel component)
        {
            Component = component;
        }
    }
}