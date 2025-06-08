using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using WordLearningWpfApp.Services;
using WordLearningWpfApp.Models;
using WordLearningWpfApp.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace WordLearningWpfApp.ViewModels
{
    public class LoginViewModel : ViewModelBase, IInitializable
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;
        private readonly INotificationService _notificationService;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private bool _rememberMe;
        private bool _isLoading;
        private string _errorMessage = string.Empty;

        public LoginViewModel(
            IAuthService authService,
            INavigationService navigationService,
            INotificationService notificationService)
            : base(navigationService, notificationService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));

            LoginCommand = new AsyncRelayCommand(ExecuteLogin);
            RegisterCommand = new AsyncRelayCommand(ExecuteRegister);
            ForgotPasswordCommand = new RelayCommand(async _ => await ForgotPasswordAsync());
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public bool RememberMe
        {
            get => _rememberMe;
            set => SetProperty(ref _rememberMe, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public IAsyncRelayCommand LoginCommand { get; }
        public IAsyncRelayCommand RegisterCommand { get; }
        public ICommand ForgotPasswordCommand { get; }

        public override async Task InitializeAsync()
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser != null)
                {
                    await _navigationService.NavigateToMainAsync();
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Initialization failed", ex);
            }
        }

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Email) && 
                   !string.IsNullOrWhiteSpace(Password) && 
                   !IsLoading;
        }

        private async Task ExecuteLogin()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Please enter both email and password.";
                    return;
                }

                var user = await _authService.LoginAsync(Email, Password);
                if (user != null)
                {
                    if (RememberMe)
                    {
                        await _authService.SaveCredentialsAsync(Email, Password);
                    }
                    await _navigationService.NavigateToMainAsync();
                }
                else
                {
                    ErrorMessage = "Invalid email or password.";
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Login failed", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExecuteRegister()
        {
            try
            {
                await _navigationService.NavigateToRegisterAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Navigation failed", ex);
            }
        }

        private async Task ForgotPasswordAsync()
        {
            try
            {
                await _navigationService.NavigateToForgotPasswordAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Navigation failed", ex);
            }
        }

        private void ClearError()
        {
            ErrorMessage = string.Empty;
        }
    }
} 