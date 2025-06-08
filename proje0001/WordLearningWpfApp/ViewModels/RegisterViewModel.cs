using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using WordLearningWpfApp.Interfaces;
using WordLearningWpfApp.Models;
using WordLearningWpfApp.Services;

namespace WordLearningWpfApp.ViewModels
{
    public class RegisterViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _username = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isBusy;
        private int _dailyWordLimit = 10; // Default

        public RegisterViewModel(
            IAuthService authService,
            INavigationService navigationService,
            INotificationService notificationService)
            : base(navigationService, notificationService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            
            RegisterCommand = new RelayCommand(async _ => await RegisterAsync());
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

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged(nameof(ConfirmPassword));
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public int DailyWordLimit
        {
            get => _dailyWordLimit;
            set => SetProperty(ref _dailyWordLimit, value);
        }

        public ICommand RegisterCommand { get; }
        public ICommand BackToLoginCommand { get; }

        private async Task RegisterAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) ||
                    string.IsNullOrWhiteSpace(ConfirmPassword) || string.IsNullOrWhiteSpace(Username))
                {
                    ErrorMessage = "Please fill in all fields.";
                    return;
                }

                if (Password != ConfirmPassword)
                {
                    ErrorMessage = "Passwords do not match.";
                    return;
                }

                var result = await _authService.RegisterAsync(Email, Password, Username);
                if (result)
                {
                    await ShowSuccessAsync("Registration successful", "You can now log in with your credentials.");
                    await _navigationService.NavigateToLoginAsync();
                }
                else
                {
                    ErrorMessage = "Registration failed. Please try again.";
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Registration failed", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task BackToLoginAsync()
        {
            await _navigationService.NavigateToLoginAsync();
        }
    }
} 