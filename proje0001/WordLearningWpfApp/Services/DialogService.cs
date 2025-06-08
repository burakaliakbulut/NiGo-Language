using System;
using System.Threading.Tasks;
using System.Windows;
using MaterialDesignThemes.Wpf;

namespace WordLearningWpfApp.Services
{
    public class DialogService : IDialogService
    {
        private readonly DialogHost _dialogHost;

        public DialogService(DialogHost dialogHost)
        {
            _dialogHost = dialogHost ?? throw new ArgumentNullException(nameof(dialogHost));
        }

        public async Task<T> ShowDialogAsync<T>(object content)
        {
            var result = await _dialogHost.ShowDialog(content);
            return (T)result;
        }

        public async Task ShowDialogAsync(object content)
        {
            await _dialogHost.ShowDialog(content);
        }

        public void CloseDialog()
        {
            _dialogHost.IsOpen = false;
        }

        public async Task<bool> ShowConfirmationAsync(string title, string message)
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

        public async Task ShowErrorAsync(string message)
        {
            MessageBox.Show(
                message,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }

        public async Task ShowInfoAsync(string message)
        {
            MessageBox.Show(
                message,
                "Information",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        public async Task<bool> ShowDialogAsync(string title, string message)
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            return result == MessageBoxResult.OK;
        }

        public async Task<string> ShowInputDialogAsync(string title, string message)
        {
            // TODO: Implement a proper input dialog
            return await Task.FromResult(string.Empty);
        }

        public async Task<T> ShowCustomDialogAsync<T>(string title, object content)
        {
            // TODO: Implement a proper custom dialog
            return await Task.FromResult(default(T));
        }

        public async Task ShowSuccessAsync(string message)
        {
            MessageBox.Show(
                message,
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
    }
} 