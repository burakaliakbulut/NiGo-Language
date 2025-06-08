using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using WordLearningWpfApp.Interfaces;
using WordLearningWpfApp.Models;
using WordLearningWpfApp.Services;

namespace WordLearningWpfApp.ViewModels
{
    public class ProgressViewModel : ViewModelBase
    {
        private readonly IStatisticsService _statisticsService;
        private readonly IWordService _wordService;
        private readonly User _user;

        private ObservableCollection<Word> _learnedWords = new();
        private ObservableCollection<Word> _learningWords = new();
        private ObservableCollection<Word> _difficultWords = new();
        private Statistics _statistics = new();
        private Dictionary<string, int> _categoryProgress = new();
        private Dictionary<WordDifficulty, int> _difficultyProgress = new();
        private int _learningStreak;
        private Dictionary<DateTime, int> _dailyProgress = new();

        public ObservableCollection<Word> LearnedWords
        {
            get => _learnedWords;
            set => SetProperty(ref _learnedWords, value);
        }

        public ObservableCollection<Word> LearningWords
        {
            get => _learningWords;
            set => SetProperty(ref _learningWords, value);
        }

        public ObservableCollection<Word> DifficultWords
        {
            get => _difficultWords;
            set => SetProperty(ref _difficultWords, value);
        }

        public Statistics Statistics
        {
            get => _statistics;
            set => SetProperty(ref _statistics, value);
        }

        public Dictionary<string, int> CategoryProgress
        {
            get => _categoryProgress;
            set => SetProperty(ref _categoryProgress, value);
        }

        public Dictionary<WordDifficulty, int> DifficultyProgress
        {
            get => _difficultyProgress;
            set => SetProperty(ref _difficultyProgress, value);
        }

        public int LearningStreak
        {
            get => _learningStreak;
            set => SetProperty(ref _learningStreak, value);
        }

        public Dictionary<DateTime, int> DailyProgress
        {
            get => _dailyProgress;
            set => SetProperty(ref _dailyProgress, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand BackCommand { get; }

        public ProgressViewModel(
            IStatisticsService statisticsService,
            IWordService wordService,
            INavigationService navigationService,
            INotificationService notificationService,
            User user)
            : base(navigationService, notificationService)
        {
            _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
            _wordService = wordService ?? throw new ArgumentNullException(nameof(wordService));
            _user = user ?? throw new ArgumentNullException(nameof(user));

            RefreshCommand = new RelayCommand(async () => await LoadProgressAsync());
            BackCommand = new RelayCommand(async () => await NavigateToMainAsync());
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await LoadProgressAsync();
        }

        private async Task LoadProgressAsync()
        {
            try
            {
                IsBusy = true;

                // Load statistics
                Statistics = await _statisticsService.GetUserStatisticsAsync(_user.Id);
                CategoryProgress = await _statisticsService.GetCategoryStatisticsAsync(_user.Id);
                DifficultyProgress = await _statisticsService.GetDifficultyStatisticsAsync(_user.Id);
                LearningStreak = await _statisticsService.GetLearningStreakAsync(_user.Id);
                DailyProgress = await _statisticsService.GetDailyActivityAsync(_user.Id, 30);

                // Load words
                var learnedWords = await _wordService.GetLearnedWordsAsync(_user.Id);
                var difficultWords = await _wordService.GetDifficultWordsAsync(_user.Id);

                LearnedWords.Clear();
                foreach (var word in learnedWords)
                {
                    LearnedWords.Add(word);
                }

                DifficultWords.Clear();
                foreach (var word in difficultWords)
                {
                    DifficultWords.Add(word);
                }

                // Calculate learning words (words that are neither learned nor difficult)
                var allWords = await _wordService.GetAllWordsAsync(_user.Id);
                var learnedWordIds = learnedWords.Select(w => w.Id).ToHashSet();
                var difficultWordIds = difficultWords.Select(w => w.Id).ToHashSet();

                LearningWords.Clear();
                foreach (var word in allWords)
                {
                    if (!learnedWordIds.Contains(word.Id) && !difficultWordIds.Contains(word.Id))
                    {
                        LearningWords.Add(word);
                    }
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error loading progress information", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task NavigateToMainAsync()
        {
            await _navigationService.NavigateToMainAsync();
        }
    }
} 