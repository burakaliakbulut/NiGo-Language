using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Proje0001.Services;
using Proje0001.Models;

namespace Proje0001.ViewModels
{
    public class LoginViewModel : ViewModelBase, IInitializable
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;
        private readonly INotificationService _notificationService;
        private readonly ICurrentUser _currentUser;

        private string _email;
        private string _password;
        private bool _isLoading;
        private string _errorMessage;

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
                ClearError();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
                ClearError();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand ForgotPasswordCommand { get; }

        public LoginViewModel(
            IAuthService authService,
            INavigationService navigationService,
            INotificationService notificationService,
            ICurrentUser currentUser)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));

            LoginCommand = new RelayCommand(async _ => await LoginAsync(), _ => CanLogin());
            RegisterCommand = new RelayCommand(async _ => await NavigateToRegisterAsync());
            ForgotPasswordCommand = new RelayCommand(async _ => await NavigateToForgotPasswordAsync());
        }

        public async Task InitializeAsync(object parameter = null)
        {
            try
            {
                if (_currentUser.IsAuthenticated)
                {
                    await _navigationService.NavigateToAsync("Main");
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("Failed to initialize login view", ex);
            }
        }

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !IsLoading;
        }

        private async Task LoginAsync()
        {
            if (!CanLogin())
                return;

            try
            {
                IsLoading = true;
                ClearError();

                var result = await _authService.LoginAsync(Email, Password);
                if (result.Success)
                {
                    await _navigationService.NavigateToAsync("Main");
                }
                else
                {
                    ErrorMessage = result.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("Login failed", ex);
                ErrorMessage = "An unexpected error occurred. Please try again.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task NavigateToRegisterAsync()
        {
            try
            {
                await _navigationService.NavigateToAsync("Register");
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("Navigation failed", ex);
            }
        }

        private async Task NavigateToForgotPasswordAsync()
        {
            try
            {
                await _navigationService.NavigateToAsync("ForgotPassword");
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("Navigation failed", ex);
            }
        }

        private void ClearError()
        {
            ErrorMessage = null;
        }
    }
} 