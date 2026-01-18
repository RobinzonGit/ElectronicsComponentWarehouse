using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace ElectronicsComponentWarehouse.Desktop.Client.Services
{
    /// <summary>
    /// Сервис для управления диалоговыми окнами
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Показать окно сообщения
        /// </summary>
        Task ShowMessageAsync(string title, string message);

        /// <summary>
        /// Показать окно подтверждения
        /// </summary>
        Task<bool> ShowConfirmationAsync(string title, string message);
    }

    /// <summary>
    /// Реализация сервиса диалогов
    /// </summary>
    public class DialogService : IDialogService
    {
        public async Task ShowMessageAsync(string title, string message)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        public async Task<bool> ShowConfirmationAsync(string title, string message)
        {
            return await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
                return result == MessageBoxResult.Yes;
            });
        }
    }
}