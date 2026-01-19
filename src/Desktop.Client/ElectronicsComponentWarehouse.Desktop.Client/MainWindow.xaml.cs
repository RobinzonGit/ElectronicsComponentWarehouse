using ElectronicsComponentWarehouse.Desktop.Client.Models.Categories;
using ElectronicsComponentWarehouse.Desktop.Client.Models.Components;
using ElectronicsComponentWarehouse.Desktop.Client.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ElectronicsComponentWarehouse.Desktop.Client
{
    public partial class MainWindow : Window
    {
        private readonly IAuthService _authService;
        private readonly IComponentService _componentService;
        private readonly ICategoryService _categoryService;
        private readonly IApiClient _apiClient;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        private ObservableCollection<ComponentModel> _components = new();
        private ObservableCollection<CategoryModel> _categories = new();
        private CategoryModel? _selectedCategory;
        private ComponentModel? _selectedComponent;

        private System.Windows.Threading.DispatcherTimer _timer;

        public MainWindow(
            IAuthService authService,
            IComponentService componentService,
            ICategoryService categoryService,
            IApiClient apiClient,
            IConfiguration configuration,
            IServiceProvider serviceProvider)
        {
            InitializeComponent();

            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _componentService = componentService ?? throw new ArgumentNullException(nameof(componentService));
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            // Подписываемся на события
            _authService.AuthenticationChanged += OnAuthenticationChanged;

            // Настраиваем DataGrid
            ComponentsDataGrid.ItemsSource = _components;

            // Запускаем таймер для обновления времени
            _timer = new System.Windows.Threading.DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // Загружаем данные при запуске
            Loaded += async (s, e) => await InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                SetStatus("Initializing application...", Brushes.Blue);

                // Проверяем подключение к API
                var isConnected = await _apiClient.CheckConnectionAsync();
                ConnectionStatusText.Text = isConnected ? "Connected" : "Disconnected";
                ConnectionStatusText.Foreground = isConnected ? Brushes.Green : Brushes.Red;

                if (!isConnected)
                {
                    SetStatus("Cannot connect to API server. Please check connection.", Brushes.Red);
                    return;
                }

                // Пытаемся восстановить сессию
                var hasSession = await _authService.LoadSavedSessionAsync();

                if (hasSession && _authService.IsAuthenticated)
                {
                    UpdateUserInfo();
                    await LoadCategoriesAsync();
                    SetStatus("Ready", Brushes.Green);
                }
                else
                {
                    ShowLoginDialog();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during application initialization");
                SetStatus($"Initialization error: {ex.Message}", Brushes.Red);
                MessageBox.Show($"Failed to initialize application: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnAuthenticationChanged(object? sender, bool isAuthenticated)
        {
            Dispatcher.Invoke(() =>
            {
                if (isAuthenticated)
                {
                    UpdateUserInfo();
                    _ = LoadCategoriesAsync();
                    SetStatus("User authenticated", Brushes.Green);
                }
                else
                {
                    UserInfoText.Text = "Not logged in";
                    LoginButton.Content = "Login";
                    _components.Clear();
                    _categories.Clear();
                    CategoriesTreeView.ItemsSource = null;
                    SetStatus("User logged out", Brushes.Orange);
                }
            });
        }

        private void UpdateUserInfo()
        {
            if (_authService.CurrentUser != null)
            {
                UserInfoText.Text = $"{_authService.CurrentUser.DisplayName} ({_authService.CurrentUser.Role})";
                LoginButton.Content = "Logout";

                // Обновляем видимость кнопок в зависимости от роли
                bool isAdmin = _authService.IsAdmin;
                AddCategoryButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
                DeleteComponentButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
                EditComponentButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Visible;
            }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                SetStatus("Loading categories...", Brushes.Blue);

                var categories = await _categoryService.GetCategoryHierarchyAsync();

                _categories.Clear();
                foreach (var category in categories)
                {
                    _categories.Add(category);
                }

                CategoriesTreeView.ItemsSource = _categories;

                SetStatus($"Loaded {categories.Count()} categories", Brushes.Green);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading categories");
                SetStatus($"Error loading categories: {ex.Message}", Brushes.Red);
            }
        }

        private async Task LoadComponentsAsync(int? categoryId = null)
        {
            try
            {
                SetStatus("Loading components...", Brushes.Blue);
                ShowLoading(true);

                var components = categoryId.HasValue
                    ? await _componentService.GetComponentsByCategoryIdAsync(categoryId.Value)
                    : await _componentService.GetAllComponentsAsync();

                _components.Clear();
                foreach (var component in components)
                {
                    _components.Add(component);
                }

                ComponentsCountText.Text = $"{_components.Count} components";

                if (_selectedCategory != null)
                {
                    CategoryInfoText.Text = $"Category: {_selectedCategory.Name}";
                }
                else
                {
                    CategoryInfoText.Text = "All components";
                }

                SetStatus($"Loaded {_components.Count} components", Brushes.Green);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading components");
                SetStatus($"Error loading components: {ex.Message}", Brushes.Red);
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private async void CategoriesTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _selectedCategory = e.NewValue as CategoryModel;
            await LoadComponentsAsync(_selectedCategory?.Id);
        }

        private async void RefreshCategoriesButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadCategoriesAsync();
        }

        private async void RefreshComponentsButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadComponentsAsync(_selectedCategory?.Id);
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (_authService.IsAuthenticated)
            {
                var result = MessageBox.Show("Are you sure you want to logout?",
                    "Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    await _authService.LogoutAsync();
                }
            }
            else
            {
                ShowLoginDialog();
            }
        }

        private void ShowLoginDialog()
        {
            try
            {
                var loginDialog = new LoginWindow(_authService, _serviceProvider);
                loginDialog.Owner = this;
                loginDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                var result = loginDialog.ShowDialog();

                if (result.HasValue && result.Value)
                {
                    UpdateUserInfo();
                    _ = LoadCategoriesAsync();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error showing login dialog");
                MessageBox.Show($"Login error: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddComponentButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_authService.IsAuthenticated)
            {
                MessageBox.Show("Please login first", "Authentication Required",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var editDialog = new ComponentEditWindow(_componentService, _categoryService,
                    _authService.IsAdmin, _serviceProvider);
                editDialog.Owner = this;
                editDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                var result = editDialog.ShowDialog();

                if (result.HasValue && result.Value)
                {
                    await LoadComponentsAsync(_selectedCategory?.Id);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error adding component");
                MessageBox.Show($"Error adding component: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void EditComponentButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedComponent == null)
            {
                MessageBox.Show("Please select a component first", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!_authService.IsAuthenticated)
            {
                MessageBox.Show("Please login first", "Authentication Required",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var editDialog = new ComponentEditWindow(_componentService, _categoryService,
                    _authService.IsAdmin, _serviceProvider, _selectedComponent);
                editDialog.Owner = this;
                editDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                var result = editDialog.ShowDialog();

                if (result.HasValue && result.Value)
                {
                    await LoadComponentsAsync(_selectedCategory?.Id);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error editing component");
                MessageBox.Show($"Error editing component: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteComponentButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedComponent == null)
            {
                MessageBox.Show("Please select a component first", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!_authService.IsAdmin)
            {
                MessageBox.Show("Only administrators can delete components", "Permission Denied",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete component '{_selectedComponent.Name}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var success = await _componentService.DeleteComponentAsync(_selectedComponent.Id);

                    if (success)
                    {
                        SetStatus($"Component '{_selectedComponent.Name}' deleted", Brushes.Green);
                        await LoadComponentsAsync(_selectedCategory?.Id);
                    }
                    else
                    {
                        SetStatus("Failed to delete component", Brushes.Red);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error deleting component");
                    MessageBox.Show($"Error deleting component: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ComponentsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_selectedComponent != null && !string.IsNullOrEmpty(_selectedComponent.DatasheetLink))
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = _selectedComponent.DatasheetLink,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error opening datasheet link");
                    MessageBox.Show($"Cannot open datasheet: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ComponentsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedComponent = ComponentsDataGrid.SelectedItem as ComponentModel;
        }

        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchTerm = SearchTextBox.Text.Trim();

            if (string.IsNullOrEmpty(searchTerm))
            {
                await LoadComponentsAsync(_selectedCategory?.Id);
                return;
            }

            if (searchTerm.Length < 2) return; // Минимум 2 символа для поиска

            try
            {
                var components = await _componentService.SearchComponentsAsync(searchTerm);

                _components.Clear();
                foreach (var component in components)
                {
                    _components.Add(component);
                }

                ComponentsCountText.Text = $"{_components.Count} components found";
                SetStatus($"Found {_components.Count} components for '{searchTerm}'", Brushes.Green);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error searching components");
                SetStatus($"Search error: {ex.Message}", Brushes.Red);
            }
        }

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            CurrentTimeText.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private void SetStatus(string message, Brush color)
        {
            StatusText.Text = message;
            StatusText.Foreground = color;
            Log.Information("Status: {Message}", message);
        }

        private void ShowLoading(bool show)
        {
            LoadingProgressBar.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            LoadingProgressBar.IsIndeterminate = show;
        }

        private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Add category functionality will be implemented in Phase 6",
                "Coming Soon", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Export functionality will be implemented in Phase 9",
                "Coming Soon", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected override void OnClosed(EventArgs e)
        {
            _timer.Stop();
            _authService.AuthenticationChanged -= OnAuthenticationChanged;
            base.OnClosed(e);
        }
    }
}