using System.Windows;
using System.Windows.Controls;
using ElectronicsComponentWarehouse.Desktop.Client.ViewModels;
using ElectronicsComponentWarehouse.Desktop.Client.Models.Categories;

namespace ElectronicsComponentWarehouse.Desktop.Client
{
    public partial class MainWindow : Window
    {
        private MainViewModel? _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnWindowLoaded;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel = DataContext as MainViewModel;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_viewModel != null && e.NewValue is CategoryModel category)
            {
                _viewModel.SelectedCategory = category;
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Сохраняем настройки перед закрытием
            _viewModel?.SaveSettings();
            base.OnClosing(e);
        }
    }
}