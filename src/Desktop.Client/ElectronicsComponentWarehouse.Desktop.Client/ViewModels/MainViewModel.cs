using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ElectronicsComponentWarehouse.Desktop.Client.Models.Auth;
using ElectronicsComponentWarehouse.Desktop.Client.Models.Categories;
using ElectronicsComponentWarehouse.Desktop.Client.Services;
using ElectronicsComponentWarehouse.Desktop.Client.Services.Interfaces;
using ElectronicsComponentWarehouse.Desktop.Client.ViewModels.Categories;
using ElectronicsComponentWarehouse.Desktop.Client.ViewModels.Components;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace ElectronicsComponentWarehouse.Desktop.Client.ViewModels
{
    /// <summary>
    /// Главная ViewModel приложения
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly CurrentUserService _currentUserService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IAuthService _authService;

        [ObservableProperty]
        private string _windowTitle = "Electronics Component Warehouse";

        [ObservableProperty]
        private UserInfoModel? _currentUser;

        [ObservableProperty]
        private ObservableObject? _currentContent;

        [ObservableProperty]
        private string _statusMessage = "Готово";

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private int _progressValue;

        [ObservableProperty]
        private string _appVersion = "1.0.0";

        [ObservableProperty]
        private bool _isMenuOpen;

        public ComponentListViewModel ComponentListViewModel { get; }
        public CategoryTreeViewModel CategoryTreeViewModel { get; }
        public CategoryModel SelectedCategory { get; internal set; }

        public MainViewModel(
            CurrentUserService currentUserService,
            ComponentListViewModel componentListViewModel,
            CategoryTreeViewModel categoryTreeViewModel,
            IDialogService dialogService,
            INavigationService navigationService,
            IAuthService authService)
        {
            _currentUserService = currentUserService;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _authService = authService;

            ComponentListViewModel = componentListViewModel;
            CategoryTreeViewModel = categoryTreeViewModel;

            // Устанавливаем начальный контент
            CurrentContent = ComponentListViewModel;

            // Подписываемся на события
            _currentUserService.UserChanged += OnUserChanged;
            WeakReferenceMessenger.Default.Register<AuthenticationSuccessMessage>(this, OnAuthenticationSuccess);
            WeakReferenceMessenger.Default.Register<ComponentSavedMessage>(this, OnComponentSaved);
            WeakReferenceMessenger.Default.Register<CloseWindowMessage>(this, OnCloseWindow);

            // Инициализация текущего пользователя
            CurrentUser = _currentUserService.CurrentUser;
        }

        /// <summary>
        /// Команда показа списка компонентов
        /// </summary>
        [RelayCommand]
        private void ShowComponents()
        {
            CurrentContent = ComponentListViewModel;
            StatusMessage = "Просмотр компонентов";
            IsMenuOpen = false;
        }

        /// <summary>
        /// Команда показа дерева категорий
        /// </summary>
        [RelayCommand]
        private void ShowCategories()
        {
            CurrentContent = CategoryTreeViewModel;
            StatusMessage = "Просмотр категорий";
            IsMenuOpen = false;
        }

        /// <summary>
        /// Команда показа отчетов
        /// </summary>
        [RelayCommand]
        private async Task ShowReportsAsync()
        {
            await _dialogService.ShowMessageAsync("Отчеты",
                "Функция отчетов будет реализована в следующей версии");
            StatusMessage = "Просмотр отчетов";
            IsMenuOpen = false;
        }

        /// <summary>
        /// Команда выхода из системы
        /// </summary>
        [RelayCommand]
        private async Task LogoutAsync()
        {
            var confirm = await _dialogService.ShowConfirmationAsync(
                "Подтверждение выхода",
                "Вы уверены, что хотите выйти из системы?");

            if (confirm)
            {
                await _authService.LogoutAsync();  // Используем IAuthService напрямую
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// Команда переключения меню
        /// </summary>
        [RelayCommand]
        private void ToggleMenu()
        {
            IsMenuOpen = !IsMenuOpen;
        }

        /// <summary>
        /// Команда обновления данных
        /// </summary>
        [RelayCommand]
        private async Task RefreshDataAsync()
        {
            if (CurrentContent is ComponentListViewModel componentList)
            {
                await componentList.RefreshAsync();  // Теперь это public метод
                StatusMessage = "Данные компонентов обновлены";
            }
            else if (CurrentContent is CategoryTreeViewModel categoryTree)
            {
                await categoryTree.RefreshAsync();  // Теперь это public метод
                StatusMessage = "Данные категорий обновлены";
            }
        }

        /// <summary>
        /// Команда открытия настроек
        /// </summary>
        [RelayCommand]
        private async Task OpenSettingsAsync()
        {
            await _dialogService.ShowMessageAsync("Настройки",
                "Функция настроек будет реализована в следующей версии");
            StatusMessage = "Настройки";
        }

        /// <summary>
        /// Команда показа информации о программе
        /// </summary>
        [RelayCommand]
        private async Task AboutAsync()
        {
            await _dialogService.ShowMessageAsync("О программе",
                "Electronics Component Warehouse\n" +
                "Версия: 1.0.0\n" +
                "Учебное приложение для демонстрации лучших практик .NET разработки\n\n" +
                "Функции:\n" +
                "- Учет электронных компонентов\n" +
                "- Иерархическая система категорий\n" +
                "- Разграничение прав доступа\n" +
                "- REST API с JWT аутентификацией\n" +
                "- MVVM архитектура с WPF");
            StatusMessage = "О программе";
        }

        /// <summary>
        /// Обработчик изменения пользователя
        /// </summary>
        private void OnUserChanged(object? sender, EventArgs e)
        {
            CurrentUser = _currentUserService.CurrentUser;
            StatusMessage = CurrentUser != null
                ? $"Добро пожаловать, {CurrentUser.Username}!"
                : "Готово";
        }

        /// <summary>
        /// Обработчик успешной аутентификации
        /// </summary>
        private void OnAuthenticationSuccess(object recipient, AuthenticationSuccessMessage message)
        {
            if (message.IsAuthenticated)
            {
                CurrentUser = _currentUserService.CurrentUser;
                StatusMessage = $"Добро пожаловать, {CurrentUser?.Username}!";
            }
        }

        /// <summary>
        /// Обработчик сохранения компонента
        /// </summary>
        private async void OnComponentSaved(object recipient, ComponentSavedMessage message)
        {
            var action = message.WasEdit ? "обновлен" : "добавлен";
            StatusMessage = $"Компонент '{message.Component.Name}' {action}";

            // Обновляем список компонентов
            await ComponentListViewModel.RefreshAsync();  // Теперь это public метод
        }

        /// <summary>
        /// Обработчик закрытия окна
        /// </summary>
        private void OnCloseWindow(object recipient, CloseWindowMessage message)
        {
            // TODO: Реализовать логику закрытия окон
        }

        /// <summary>
        /// Сохранение настроек приложения
        /// </summary>
        public void SaveSettings()
        {
            // TODO: Реализовать сохранение настроек
            StatusMessage = "Настройки сохранены";
        }
    }
}