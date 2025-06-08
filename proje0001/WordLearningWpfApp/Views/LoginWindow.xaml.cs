using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WordLearningWpfApp.Interfaces;
using WordLearningWpfApp.Models;
using WordLearningWpfApp.Services;
using WordLearningWpfApp.ViewModels;

namespace WordLearningWpfApp.Views
{
    public partial class LoginWindow : BaseWindow
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;
        private readonly INotificationService _notificationService;
        private readonly IWordService _wordService;
        private readonly IStatisticsService _statisticsService;
        private readonly IExamService _examService;
        private readonly LoginViewModel _viewModel;

        public LoginWindow(
            IAuthService authService,
            INavigationService navigationService,
            INotificationService notificationService,
            IWordService wordService,
            IStatisticsService statisticsService,
            IExamService examService)
        {
            _authService = authService;
            _navigationService = navigationService;
            _notificationService = notificationService;
            _wordService = wordService;
            _statisticsService = statisticsService;
            _examService = examService;

            _viewModel = new LoginViewModel(authService, navigationService, notificationService);
            DataContext = _viewModel;

            new InitializeComponent();
        }

        private void ShowLoadingIndicator()
        {
            if (LoadingProgressBar != null)
            {
                LoadingProgressBar.Visibility = Visibility.Visible;
            }
        }

        private void HideLoadingIndicator()
        {
            if (LoadingProgressBar != null)
            {
                LoadingProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowError(string message)
        {
            if (ErrorTextBlock != null)
            {
                ErrorTextBlock.Text = message;
                ErrorTextBlock.Visibility = Visibility.Visible;
            }
        }

        private void HideError()
        {
            if (ErrorTextBlock != null)
            {
                ErrorTextBlock.Text = string.Empty;
                ErrorTextBlock.Visibility = Visibility.Collapsed;
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(EmailTextBox.Text) || string.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    ShowError("Please enter both email and password");
                    return;
                }

                var user = await _authService.LoginAsync(EmailTextBox.Text, PasswordBox.Password);
                if (user != null)
                {
                    await _navigationService.NavigateToMainAsync();
                }
                else
                {
                    ShowError("Invalid email or password");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error during login: {ex.Message}");
            }
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            await _navigationService.NavigateToRegisterAsync();
        }

        private void ForgotPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ForgotPasswordCommand.Execute(null);
        }

        private void TogglePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Visibility == Visibility.Visible)
            {
                PasswordBox.Visibility = Visibility.Collapsed;
                PasswordTextBox.Visibility = Visibility.Visible;
                PasswordTextBox.Text = PasswordBox.Password;
                TogglePasswordButton.Content = new PackIcon { Kind = PackIconKind.EyeOff };
            }
            else
            {
                PasswordBox.Visibility = Visibility.Visible;
                PasswordTextBox.Visibility = Visibility.Collapsed;
                PasswordBox.Password = PasswordTextBox.Text;
                TogglePasswordButton.Content = new PackIcon { Kind = PackIconKind.Eye };
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.Password = PasswordBox.Password;
            }
        }
    }
} 