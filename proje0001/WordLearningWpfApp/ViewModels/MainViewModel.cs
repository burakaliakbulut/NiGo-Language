using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using WordLearningWpfApp.Interfaces;
using WordLearningWpfApp.Models;
using WordLearningWpfApp.Services;
using System.Linq;

namespace WordLearningWpfApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private readonly IWordService _wordService;
        private readonly IStatisticsService _statisticsService;
        private readonly IAuthService _authService;

        private User? _user;
        private string _username = string.Empty;
        private string _email = string.Empty;
        private int _dailyWordLimit = 10;
        private int _wordsLearned;
        private int _wordsToLearn;
        private int _streak;
        private ObservableCollection<Word> _dailyWords = new();
        private bool _isBusy;

        public MainViewModel(
            IUserService userService,
            IWordService wordService,
            IStatisticsService statisticsService,
            IAuthService authService,
            INavigationService navigationService,
            INotificationService notificationService)
            : base(navigationService, notificationService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _wordService = wordService ?? throw new ArgumentNullException(nameof(wordService));
            _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            StartLearningCommand = new RelayCommand(async _ => await StartLearningAsync());
            StartExamCommand = new RelayCommand(async _ => await StartExamAsync());
            ViewStatisticsCommand = new RelayCommand(async _ => await ViewStatisticsAsync());
            ViewDictionaryCommand = new RelayCommand(async _ => await ViewDictionaryAsync());
            LogoutCommand = new RelayCommand(async _ => await LogoutAsync());
            SettingsCommand = new RelayCommand(async _ => await OpenSettingsAsync());

            LoadDataAsync().ConfigureAwait(false);
        }

        public User? User
        {
            get => _user;
            private set
            {
                _user = value;
                OnPropertyChanged(nameof(User));
            }
        }

        public string Username
        {
            get => _username;
            private set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public string Email
        {
            get => _email;
            private set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        public int DailyWordLimit
        {
            get => _dailyWordLimit;
            private set
            {
                _dailyWordLimit = value;
                OnPropertyChanged(nameof(DailyWordLimit));
            }
        }

        public int WordsLearned
        {
            get => _wordsLearned;
            private set
            {
                _wordsLearned = value;
                OnPropertyChanged(nameof(WordsLearned));
            }
        }

        public int WordsToLearn
        {
            get => _wordsToLearn;
            private set
            {
                _wordsToLearn = value;
                OnPropertyChanged(nameof(WordsToLearn));
            }
        }

        public int Streak
        {
            get => _streak;
            private set
            {
                _streak = value;
                OnPropertyChanged(nameof(Streak));
            }
        }

        public ObservableCollection<Word> DailyWords
        {
            get => _dailyWords;
            private set
            {
                _dailyWords = value;
                OnPropertyChanged(nameof(DailyWords));
            }
        }

        public ICommand StartLearningCommand { get; }
        public ICommand StartExamCommand { get; }
        public ICommand ViewStatisticsCommand { get; }
        public ICommand ViewDictionaryCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand SettingsCommand { get; }

        private async Task LoadDataAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    await ShowErrorAsync("User not found");
                    await _navigationService.NavigateToLoginAsync();
                    return;
                }

                User = currentUser;
                Username = currentUser.Username;
                Email = currentUser.Email;

                var preferences = await _userService.GetUserPreferencesAsync(currentUser.Id);
                DailyWordLimit = preferences?.DailyWordCount ?? 10;

                var progress = await _statisticsService.GetProgressDataAsync(currentUser.Id);
                WordsLearned = progress?.WordsLearned ?? 0;
                WordsToLearn = progress?.WordsToLearn ?? 0;

                var streak = await _statisticsService.GetLearningStreakAsync(currentUser.Id);
                Streak = streak;

                var dailyWords = await _wordService.GetWordsForReviewAsync(currentUser.Id);
                DailyWords = new ObservableCollection<Word>(dailyWords.Take(DailyWordLimit));
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error loading data", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task StartLearningAsync()
        {
            try
            {
                await _navigationService.NavigateToDailyWordsAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error starting learning session", ex);
            }
        }

        private async Task StartExamAsync()
        {
            try
            {
                await _navigationService.NavigateToExamAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error starting exam", ex);
            }
        }

        private async Task ViewStatisticsAsync()
        {
            try
            {
                await _navigationService.NavigateToStatisticsAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error viewing statistics", ex);
            }
        }

        private async Task ViewDictionaryAsync()
        {
            try
            {
                await _navigationService.NavigateToDictionaryAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error viewing dictionary", ex);
            }
        }

        private async Task LogoutAsync()
        {
            try
            {
                await _authService.LogoutAsync();
                await _navigationService.NavigateToLoginAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error logging out", ex);
            }
        }

        private async Task OpenSettingsAsync()
        {
            try
            {
                await _navigationService.NavigateToSettingsAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error opening settings", ex);
            }
        }
    }
} 