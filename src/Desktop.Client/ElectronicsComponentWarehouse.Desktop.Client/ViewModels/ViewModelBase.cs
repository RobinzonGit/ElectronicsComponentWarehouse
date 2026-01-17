using CommunityToolkit.Mvvm.ComponentModel;

namespace ElectronicsComponentWarehouse.Desktop.Client.ViewModels
{
    public class ViewModelBase : ObservableObject
    {
        private string _title = string.Empty;
        private bool _isBusy;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }
    }
}
