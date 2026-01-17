using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectronicsComponentWarehouse.Desktop.Client.Models.Auth;
using ElectronicsComponentWarehouse.Desktop.Client.Models.Categories;
using ElectronicsComponentWarehouse.Desktop.Client.Models.Components;
using ElectronicsComponentWarehouse.Desktop.Client.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace ElectronicsComponentWarehouse.Desktop.Client.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly IComponentService _componentService;
        private readonly ICategoryService _categoryService;

        [ObservableProperty]
        private string _windowTitle = "Electronics Component Warehouse";

        [ObservableProperty]
        private UserInfoModel? _currentUser;

        [ObservableProperty]
        private ObservableCollection<CategoryModel> _categories = new();

        [ObservableProperty]
        private ObservableCollection<ComponentModel> _components = new();

        [ObservableProperty]
        private CategoryModel? _selectedCategory;

        [ObservableProperty]
        private ComponentModel? _selectedComponent;

        [ObservableProperty]
        private string _statusMessage = "Готово";

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private int _progressValue;

        [ObservableProperty]
        private bool _isIndeterminate;

        [ObservableProperty]
        private string _appVersion = "1.0.0";

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _selectedCategoryPath = "Все категории";

        // Команды
        public IAsyncRelayCommand ShowComponentsCommand { get; }
        public IRelayCommand ShowCategoriesCommand { get; }
        public IRelayCommand ShowReportsCommand { get; }
        public IRelayCommand LogoutCommand { get; }
        public IAsyncRelayCommand RefreshCategoriesCommand { get; }
        public IAsyncRelayCommand RefreshComponentsCommand { get; }
        public IAsyncRelayCommand SearchCommand { get; }
        public IRelayCommand AddCategoryCommand { get; }
        public IRelayCommand AddComponentCommand { get; }
        public IRelayCommand EditComponentCommand { get; }
        public IRelayCommand DeleteComponentCommand { get; }
        public IRelayCommand OpenDatasheetCommand { get; }
        public IRelayCommand QuickEditComponentCommand { get; }

        public MainViewModel(
            IAuthService authService,
            IComponentService componentService,
            ICategoryService categoryService)
        {
            _authService = authService;
            _componentService = componentService;
            _categoryService = categoryService;

            // Инициализация команд
            ShowComponentsCommand = new AsyncRelayCommand(ShowComponentsAsync);
            ShowCategoriesCommand = new RelayCommand(ShowCategories);
            ShowReportsCommand = new RelayCommand(ShowReports);
            LogoutCommand = new RelayCommand(Logout);
            RefreshCategoriesCommand = new AsyncRelayCommand(RefreshCategoriesAsync);
            RefreshComponentsCommand = new AsyncRelayCommand(RefreshComponentsAsync);
            SearchCommand = new AsyncRelayCommand(SearchComponentsAsync);
            AddCategoryCommand = new RelayCommand(AddCategory);
            AddComponentCommand = new RelayCommand(AddComponent);
            EditComponentCommand = new RelayCommand(EditComponent);
            DeleteComponentCommand = new RelayCommand(DeleteComponent);
            OpenDatasheetCommand = new RelayCommand<string>(OpenDatasheet);
            QuickEditComponentCommand = new RelayCommand<ComponentModel>(QuickEditComponent);

            // Загрузка данных при старте
            LoadInitialData();
        }

        private async Task ShowComponentsAsync()
        {
            await RefreshComponentsAsync();
        }

        private void ShowCategories()
        {
            StatusMessage = "Просмотр категорий";
        }

        private void ShowReports()
        {
            StatusMessage = "Просмотр отчетов";
        }

        private void Logout()
        {
            _ = _authService.LogoutAsync();
            Application.Current.Shutdown();
        }

        private async Task RefreshCategoriesAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Загрузка категорий...";

                var categories = await _categoryService.GetCategoryHierarchyAsync();
                Categories.Clear();

                foreach (var category in categories)
                {
                    Categories.Add(category);
                }

                StatusMessage = $"Загружено {Categories.Count} категорий";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки категорий: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task RefreshComponentsAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Загрузка компонентов...";

                var components = await _componentService.GetAllComponentsAsync();
                Components.Clear();

                foreach (var component in components)
                {
                    Components.Add(component);
                }

                StatusMessage = $"Загружено {Components.Count} компонентов";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки компонентов: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SearchComponentsAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await RefreshComponentsAsync();
                return;
            }

            try
            {
                IsBusy = true;
                StatusMessage = "Поиск компонентов...";

                var components = await _componentService.SearchComponentsAsync(SearchText);
                Components.Clear();

                foreach (var component in components)
                {
                    Components.Add(component);
                }

                StatusMessage = $"Найдено {Components.Count} компонентов";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка поиска: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void AddCategory()
        {
            StatusMessage = "Добавление категории";
            // TODO: Реализовать диалог добавления категории
        }

        private void AddComponent()
        {
            StatusMessage = "Добавление компонента";
            // TODO: Реализовать диалог добавления компонента
        }

        private void EditComponent()
        {
            if (SelectedComponent != null)
            {
                StatusMessage = $"Редактирование компонента: {SelectedComponent.Name}";
                // TODO: Реализовать диалог редактирования компонента
            }
        }

        private void DeleteComponent()
        {
            if (SelectedComponent != null)
            {
                StatusMessage = $"Удаление компонента: {SelectedComponent.Name}";
                // TODO: Реализовать подтверждение и удаление
            }
        }

        private void OpenDatasheet(string? datasheetLink)
        {
            if (!string.IsNullOrEmpty(datasheetLink))
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = datasheetLink,
                        UseShellExecute = true
                    });
                    StatusMessage = "Открытие даташита";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Ошибка открытия даташита: {ex.Message}";
                }
            }
        }

        private void QuickEditComponent(ComponentModel? component)
        {
            if (component != null)
            {
                StatusMessage = $"Быстрое редактирование: {component.Name}";
                // TODO: Реализовать быстрое редактирование
            }
        }

        private async void LoadInitialData()
        {
            // Проверяем авторизацию
            if (_authService.IsAuthenticated)
            {
                CurrentUser = _authService.CurrentUser;

                // Загружаем начальные данные
                await RefreshCategoriesAsync();
                await RefreshComponentsAsync();
            }
        }

        public void SaveSettings()
        {
            // Сохранение настроек приложения
            StatusMessage = "Настройки сохранены";
        }

        partial void OnSelectedCategoryChanged(CategoryModel? value)
        {
            if (value != null)
            {
                SelectedCategoryPath = value.Name;
                LoadComponentsByCategory(value.Id);
            }
            else
            {
                SelectedCategoryPath = "Все категории";
            }
        }

        private async void LoadComponentsByCategory(int categoryId)
        {
            try
            {
                IsBusy = true;
                StatusMessage = $"Загрузка компонентов категории...";

                var components = await _componentService.GetComponentsByCategoryIdAsync(categoryId);
                Components.Clear();

                foreach (var component in components)
                {
                    Components.Add(component);
                }

                StatusMessage = $"Загружено {Components.Count} компонентов";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки компонентов категории: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}