using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using WordLearningWpfApp.Interfaces;
using WordLearningWpfApp.Models;
using WordLearningWpfApp.Services;

namespace WordLearningWpfApp.ViewModels
{
    public class ForgotPasswordViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private string _email = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isBusy;

        public ForgotPasswordViewModel(
            IAuthService authService,
            INavigationService navigationService,
            INotificationService notificationService)
            : base(navigationService, notificationService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            
            ResetPasswordCommand = new RelayCommand(async _ => await ResetPasswordAsync());
            BackToLoginCommand = new RelayCommand(async _ => await BackToLoginAsync());
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        public ICommand ResetPasswordCommand { get; }
        public ICommand BackToLoginCommand { get; }

        private async Task ResetPasswordAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                if (string.IsNullOrWhiteSpace(Email))
                {
                    ErrorMessage = "Please enter your email address.";
                    return;
                }

                var result = await _authService.ResetPasswordAsync(Email);
                if (result)
                {
                    await ShowSuccessAsync("Password reset", "If an account exists with this email, you will receive password reset instructions.");
                    await _navigationService.NavigateToLoginAsync();
                }
                else
                {
                    ErrorMessage = "Failed to process password reset request.";
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Password reset failed", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task BackToLoginAsync()
        {
            // TODO: Navigate back
        }

        private void ExecuteBack()
        {
            _navigationService.NavigateToLogin();
        }
    }
} 