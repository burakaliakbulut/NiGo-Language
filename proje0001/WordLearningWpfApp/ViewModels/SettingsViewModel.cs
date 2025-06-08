using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using WordLearningWpfApp.Interfaces;
using WordLearningWpfApp.Models;
using WordLearningWpfApp.Services;

namespace WordLearningWpfApp.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private readonly string _userId;

        private User _user = null!;
        private string _dailyWordLimit = "10";
        private bool _isDarkMode;
        private bool _isNotificationsEnabled;
        private string _language = "en";
        private bool _isLoading;

        public User User
        {
            get => _user;
            set => SetProperty(ref _user, value);
        }

        public string DailyWordLimit
        {
            get => _dailyWordLimit;
            set
            {
                if (int.TryParse(value, out var limit) && limit > 0)
                {
                    _dailyWordLimit = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set => SetProperty(ref _isDarkMode, value);
        }

        public bool IsNotificationsEnabled
        {
            get => _isNotificationsEnabled;
            set => SetProperty(ref _isNotificationsEnabled, value);
        }

        public string Language
        {
            get => _language;
            set => SetProperty(ref _language, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand BackCommand { get; }

        public SettingsViewModel(
            IUserService userService,
            INavigationService navigationService,
            INotificationService notificationService,
            string userId)
            : base(navigationService, notificationService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _userId = userId ?? throw new ArgumentNullException(nameof(userId));

            SaveCommand = new RelayCommand(async () => await ExecuteSaveAsync());
            BackCommand = new RelayCommand(async () => await ExecuteBackAsync());
        }

        public override async Task InitializeAsync()
        {
            await LoadUserSettingsAsync();
        }

        private async Task LoadUserSettingsAsync()
        {
            try
            {
                IsLoading = true;
                User = await _userService.GetUserByIdAsync(_userId);
                if (User != null)
                {
                    DailyWordLimit = User.DailyWordLimit.ToString();
                    IsDarkMode = User.IsDarkMode;
                    IsNotificationsEnabled = User.IsNotificationsEnabled;
                    Language = User.Language;
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error loading settings", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExecuteSaveAsync()
        {
            if (User == null) return;

            try
            {
                IsLoading = true;
                if (!int.TryParse(DailyWordLimit, out var limit))
                {
                    await ShowErrorAsync("Invalid daily word limit");
                    return;
                }
                User.DailyWordLimit = limit;
                User.IsDarkMode = IsDarkMode;
                User.IsNotificationsEnabled = IsNotificationsEnabled;
                User.Language = Language;
                await _userService.UpdateUserAsync(User);
                await ShowSuccessAsync("Settings saved successfully");
                await ExecuteBackAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error saving settings", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExecuteBackAsync()
        {
            await _navigationService.NavigateToMainAsync();
        }

        public override string ErrorMessage => base.ErrorMessage;
        public override bool IsBusy => base.IsBusy;
    }
} 