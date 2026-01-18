// NavigationService.cs
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace ElectronicsComponentWarehouse.Desktop.Client.Services
{
    /// <summary>
    /// Сервис для навигации между ViewModels
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Текущая ViewModel
        /// </summary>
        ObservableObject? CurrentViewModel { get; }

        /// <summary>
        /// Событие изменения текущей ViewModel
        /// </summary>
        event EventHandler<ObservableObject?>? CurrentViewModelChanged;

        /// <summary>
        /// Перейти к указанной ViewModel
        /// </summary>
        void NavigateTo<T>() where T : ObservableObject;

        /// <summary>
        /// Перейти к указанной ViewModel с параметрами
        /// </summary>
        void NavigateTo<T>(Dictionary<string, object> parameters) where T : ObservableObject;

        /// <summary>
        /// Вернуться назад
        /// </summary>
        bool GoBack();

        /// <summary>
        /// Проверить, можно ли вернуться назад
        /// </summary>
        bool CanGoBack();
    }

    /// <summary>
    /// Реализация сервиса навигации
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Stack<ObservableObject> _backStack = new();
        private ObservableObject? _currentViewModel;

        public ObservableObject? CurrentViewModel
        {
            get => _currentViewModel;
            private set
            {
                if (_currentViewModel != value)
                {
                    _currentViewModel = value;
                    CurrentViewModelChanged?.Invoke(this, value);
                }
            }
        }

        public event EventHandler<ObservableObject?>? CurrentViewModelChanged;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void NavigateTo<T>() where T : ObservableObject
        {
            NavigateTo<T>(new Dictionary<string, object>());
        }

        public void NavigateTo<T>(Dictionary<string, object> parameters) where T : ObservableObject
        {
            try
            {
                var viewModel = _serviceProvider.GetService<T>();
                if (viewModel == null)
                {
                    // Создаем экземпляр через активатор
                    viewModel = Activator.CreateInstance<T>();
                }

                // Сохраняем текущую ViewModel в стек
                if (CurrentViewModel != null)
                {
                    _backStack.Push(CurrentViewModel);
                }

                CurrentViewModel = viewModel;
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Console.WriteLine($"Ошибка навигации: {ex.Message}");
                throw;
            }
        }

        public bool GoBack()
        {
            if (_backStack.Count > 0)
            {
                CurrentViewModel = _backStack.Pop();
                return true;
            }

            return false;
        }

        public bool CanGoBack()
        {
            return _backStack.Count > 0;
        }
    }
}