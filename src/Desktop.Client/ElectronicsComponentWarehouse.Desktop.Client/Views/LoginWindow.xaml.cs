using System.Windows;
using System.Windows.Controls;
using ElectronicsComponentWarehouse.Desktop.Client.ViewModels;

namespace ElectronicsComponentWarehouse.Desktop.Client.Views
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _viewModel;

        public LoginWindow(LoginViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            // Привязка PasswordBox к ViewModel
            PasswordBox.PasswordChanged += (s, e) => _viewModel.Password = PasswordBox.Password;

            // Установка начального пароля, если нужно
            if (!string.IsNullOrEmpty(_viewModel.Password))
            {
                PasswordBox.Password = _viewModel.Password;
            }
        }
    }
}