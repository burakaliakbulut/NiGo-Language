using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using WordLearningWpfApp.Interfaces;
using WordLearningWpfApp.Models;
using WordLearningWpfApp.Services;
using WordLearningWpfApp.Views;

namespace WordLearningWpfApp.ViewModels
{
    public class DailyWordsViewModel : ViewModelBase
    {
        private readonly IWordService _wordService;
        private readonly IAuthService _authService;
        private readonly IStatisticsService _statisticsService;
        private ObservableCollection<Word> _dailyWords = new();
        private Word? _selectedWord;
        private int _currentIndex;
        private bool _isLearning;
        private bool _isAnswerVisible;
        private string _userAnswer;
        private bool _isCorrect;

        public ObservableCollection<Word> DailyWords
        {
            get => _dailyWords;
            private set
            {
                _dailyWords = value;
                OnPropertyChanged(nameof(DailyWords));
            }
        }

        public Word? SelectedWord
        {
            get => _selectedWord;
            private set
            {
                _selectedWord = value;
                OnPropertyChanged(nameof(SelectedWord));
            }
        }

        public int CurrentIndex
        {
            get => _currentIndex;
            private set
            {
                _currentIndex = value;
                OnPropertyChanged(nameof(CurrentIndex));
            }
        }

        public bool IsLearning
        {
            get => _isLearning;
            private set
            {
                _isLearning = value;
                OnPropertyChanged(nameof(IsLearning));
            }
        }

        public ICommand StartLearningCommand { get; }
        public ICommand MarkAsLearnedCommand { get; }
        public ICommand NextWordCommand { get; }
        public ICommand BackCommand { get; }

        public DailyWordsViewModel(
            IWordService wordService,
            IAuthService authService,
            INavigationService navigationService,
            INotificationService notificationService)
            : base(navigationService, notificationService)
        {
            _wordService = wordService ?? throw new ArgumentNullException(nameof(wordService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            StartLearningCommand = new RelayCommand(async _ => await StartLearningAsync());
            MarkAsLearnedCommand = new RelayCommand(async _ => await MarkAsLearnedAsync(), _ => SelectedWord != null && IsLearning);
            NextWordCommand = new RelayCommand(async _ => await NextWordAsync(), _ => SelectedWord != null && IsLearning);
            BackCommand = new RelayCommand(async _ => await BackAsync());

            LoadDailyWordsAsync().ConfigureAwait(false);
        }

        private async Task LoadDailyWordsAsync()
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

                var words = await _wordService.GetDailyWordsAsync(currentUser.Id);
                DailyWords = new ObservableCollection<Word>(words);
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error loading daily words", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task StartLearningAsync()
        {
            if (DailyWords.Count == 0)
            {
                await ShowErrorAsync("No words available for learning");
                return;
            }

            IsLearning = true;
            CurrentIndex = 0;
            SelectedWord = DailyWords[0];
        }

        private async Task MarkAsLearnedAsync()
        {
            if (SelectedWord == null) return;

            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                var result = await _wordService.MarkWordAsLearnedAsync(SelectedWord.Id, currentUser.Id);
                if (result)
                {
                    DailyWords.Remove(SelectedWord);
                    if (DailyWords.Count == 0)
                    {
                        await ShowSuccessAsync("You have completed all daily words!");
                        await BackAsync();
                    }
                    else
                    {
                        SelectedWord = DailyWords[CurrentIndex];
                    }
                }
                else
                {
                    await ShowErrorAsync("Failed to mark word as learned");
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error marking word as learned", ex);
            }
        }

        private async Task NextWordAsync()
        {
            if (CurrentIndex < DailyWords.Count - 1)
            {
                CurrentIndex++;
                SelectedWord = DailyWords[CurrentIndex];
            }
            else
            {
                await ShowSuccessAsync("You have completed all daily words!");
                await BackAsync();
            }
        }

        private async Task BackAsync()
        {
            await _navigationService.NavigateToMainAsync();
        }
    }
} 