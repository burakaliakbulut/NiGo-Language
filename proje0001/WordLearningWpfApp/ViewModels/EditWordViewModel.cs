using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using WordLearningWpfApp.Interfaces;
using WordLearningWpfApp.Models;
using WordLearningWpfApp.Services;
using WordLearningWpfApp.Views;

namespace WordLearningWpfApp.ViewModels
{
    public class EditWordViewModel : ViewModelBase, IInitializable
    {
        private readonly IWordService _wordService;
        private readonly IAuthService _authService;
        private string _wordText = string.Empty;
        private string _translation = string.Empty;
        private string _example = string.Empty;
        private string _notes = string.Empty;
        private WordDifficulty _difficulty = WordDifficulty.Easy;
        private string _category = string.Empty;
        private string _selectedCategory = string.Empty;
        private ObservableCollection<string> _categories = [];
        private ObservableCollection<WordSample> _examples = [];
        private bool _isBusy;
        private readonly string _wordId;
        private Word _word;
        private bool _isLoading;

        public EditWordViewModel(
            IWordService wordService,
            IAuthService authService,
            INavigationService navigationService,
            INotificationService notificationService,
            string? wordId)
            : base(navigationService, notificationService)
        {
            _wordService = wordService ?? throw new ArgumentNullException(nameof(wordService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _wordId = wordId ?? throw new ArgumentNullException(nameof(wordId));

            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave());
            CancelCommand = new RelayCommand(async _ => await CancelAsync());
            AddExampleCommand = new RelayCommand(ExecuteAddExample);
            RemoveExampleCommand = new RelayCommand<WordSample>(ExecuteRemoveExample);

            LoadDataAsync().ConfigureAwait(false);
        }

        public void Initialize(string? wordId) => _wordId = wordId;

        public new async Task InitializeAsync()
        {
            if (string.IsNullOrEmpty(_wordId))
            {
                throw new InvalidOperationException("WordId must be set before initialization");
            }

            try
            {
                IsLoading = true;
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                await _notificationService.ShowErrorAsync("Kelime yüklenirken hata oluştu", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public string WordText
        {
            get => _wordText;
            set
            {
                _wordText = value;
                OnPropertyChanged(nameof(WordText));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string Translation
        {
            get => _translation;
            set
            {
                _translation = value;
                OnPropertyChanged(nameof(Translation));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string Example
        {
            get => _example;
            set
            {
                _example = value;
                OnPropertyChanged(nameof(Example));
            }
        }

        public string Notes
        {
            get => _notes;
            set
            {
                _notes = value;
                OnPropertyChanged(nameof(Notes));
            }
        }

        public WordDifficulty Difficulty
        {
            get => _difficulty;
            set
            {
                _difficulty = value;
                OnPropertyChanged(nameof(Difficulty));
            }
        }

        public string Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged(nameof(Category));
            }
        }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<WordSample> Examples
        {
            get => _examples;
            set
            {
                _examples = value;
                OnPropertyChanged();
            }
        }

        public new bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
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

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AddExampleCommand { get; }
        public ICommand RemoveExampleCommand { get; }

        public Word Word
        {
            get => _word;
            set
            {
                _word = value;
                OnPropertyChanged();
            }
        }

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

                var word = await _wordService.GetWordByIdAsync(_wordId);
                if (word == null)
                {
                    await ShowErrorAsync("Word not found");
                    await _navigationService.NavigateToWordListAsync();
                    return;
                }

                WordText = word.English;
                Translation = word.Turkish;
                Example = word.Example ?? string.Empty;
                Notes = word.Notes ?? string.Empty;
                Difficulty = word.Difficulty;
                Category = word.Category;
                SelectedCategory = word.Category;

                var categories = await _wordService.GetCategoriesAsync(currentUser.Id);
                Categories = new ObservableCollection<string>(categories);
                Examples = new ObservableCollection<WordSample>(word.Examples);
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error loading word data", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanSave()
        {
            return !IsBusy && 
                   !string.IsNullOrWhiteSpace(WordText) && 
                   !string.IsNullOrWhiteSpace(Translation) && 
                   !string.IsNullOrWhiteSpace(Category);
        }

        private async Task SaveAsync()
        {
            if (!CanSave()) return;

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

                var updatedWord = new Word
                {
                    Id = _wordId,
                    Text = WordText,
                    Translation = Translation,
                    Example = Example,
                    Notes = Notes,
                    Difficulty = Difficulty,
                    Category = Category,
                    UserId = currentUser.Id,
                    Examples = Examples.ToList()
                };

                var result = await _wordService.UpdateWordAsync(updatedWord);
                if (result != null)
                {
                    await ShowSuccessAsync("Success", "Word updated successfully");
                    await _navigationService.NavigateToWordListAsync();
                }
                else
                {
                    await ShowErrorAsync("Failed to update word");
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error updating word", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task CancelAsync()
        {
            await _navigationService.NavigateToWordListAsync();
        }

        private void ExecuteAddExample()
        {
            var example = new WordSample
            {
                WordId = _word.Id,
                Sentence = string.Empty,
                Translation = string.Empty
            };

            Examples.Add(example);
        }

        private void ExecuteRemoveExample(WordSample? example)
        {
            if (example != null)
            {
                Examples.Remove(example);
            }
        }
    }
} 