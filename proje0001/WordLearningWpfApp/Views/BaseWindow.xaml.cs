using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using WordLearningWpfApp.ViewModels;

namespace WordLearningWpfApp.Views
{
    public partial class BaseWindow : Window
    {
        private Snackbar _snackbar;
        private ProgressBar _progressBar;

        public BaseWindow()
        {
            InitializeComponent();
            Loaded += BaseWindow_Loaded;
            Closing += BaseWindow_Closing;
        }

        private async void BaseWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _snackbar = FindName("MainSnackbar") as Snackbar;
            _progressBar = FindName("MainProgressBar") as ProgressBar;

            if (DataContext is ViewModelBase viewModel)
            {
                try
                {
                    await viewModel.InitializeAsync();
                }
                catch (Exception ex)
                {
                    ShowError(ex.Message);
                }
            }
        }

        private async void BaseWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is ViewModelBase viewModel)
            {
                try
                {
                    await viewModel.CleanupAsync();
                }
                catch (Exception ex)
                {
                    ShowError(ex.Message);
                }
            }
        }

        protected void ShowMessage(string message, bool isError = false)
        {
            if (_snackbar != null)
            {
                _snackbar.MessageQueue?.Enqueue(
                    message,
                    null,
                    null,
                    null,
                    false,
                    true,
                    TimeSpan.FromSeconds(3)
                );
            }
        }

        protected void ShowError(string message)
        {
            ShowMessage(message, true);
        }

        protected void ShowLoading(bool isLoading)
        {
            if (_progressBar != null)
            {
                _progressBar.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        protected async Task ShowLoadingAsync(Func<Task> operation)
        {
            try
            {
                ShowLoading(true);
                await operation();
            }
            finally
            {
                ShowLoading(false);
            }
        }

        protected async Task<T> ShowLoadingAsync<T>(Func<Task<T>> operation)
        {
            try
            {
                ShowLoading(true);
                return await operation();
            }
            finally
            {
                ShowLoading(false);
            }
        }

        protected void ShowSuccess(string message)
        {
            ShowMessage(message, false);
        }

        protected void ShowConfirmation(string message, Action onConfirm)
        {
            var result = MessageBox.Show(
                message,
                "Onay",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                onConfirm();
            }
        }

        protected void NavigateTo(Type windowType, object? parameter = null)
        {
            var window = Activator.CreateInstance(windowType) as Window;
            if (window != null)
            {
                if (parameter != null)
                {
                    window.DataContext = parameter;
                }
                window.Show();
                Close();
            }
        }

        protected void NavigateBack()
        {
            Close();
        }
    }
} 