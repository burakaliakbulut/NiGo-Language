using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using WordLearningWpfApp.Models;
using WordLearningWpfApp.Services;
using WordLearningWpfApp.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace WordLearningWpfApp.ViewModels
{
    public class DictionaryViewModel : ViewModelBase
    {
        private readonly IWordService _wordService;
        private readonly INavigationService _navigationService;
        private readonly INotificationService _notificationService;
        private string _searchText;
        private string _selectedCategory;
        private Word _selectedWord;
        private ObservableCollection<Word> _words;
        private ObservableCollection<string> _categories;
        private bool _isLoading;

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }

        public Word SelectedWord
        {
            get => _selectedWord;
            set => SetProperty(ref _selectedWord, value);
        }

        public ObservableCollection<Word> Words
        {
            get => _words;
            set => SetProperty(ref _words, value);
        }

        public ObservableCollection<string> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public IAsyncRelayCommand SearchCommand { get; }
        public IAsyncRelayCommand AddWordCommand { get; }
        public IAsyncRelayCommand EditWordCommand { get; }
        public IAsyncRelayCommand DeleteWordCommand { get; }
        public IAsyncRelayCommand CategoryChangedCommand { get; }

        public DictionaryViewModel(
            IWordService wordService,
            INavigationService navigationService,
            INotificationService notificationService)
            : base(navigationService, notificationService)
        {
            _wordService = wordService;
            _navigationService = navigationService;
            _notificationService = notificationService;

            Words = new ObservableCollection<Word>();
            Categories = new ObservableCollection<string>();

            SearchCommand = new AsyncRelayCommand(ExecuteSearch);
            AddWordCommand = new AsyncRelayCommand(ExecuteAddWord);
            EditWordCommand = new AsyncRelayCommand(ExecuteEditWord);
            DeleteWordCommand = new AsyncRelayCommand(ExecuteDeleteWord);
            CategoryChangedCommand = new AsyncRelayCommand(ExecuteCategoryChanged);
        }

        public override async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Error initializing dictionary: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadDataAsync()
        {
            var words = await _wordService.GetWordsByUserIdAsync(CurrentUser.Id);
            var categories = await _wordService.GetCategoriesAsync(CurrentUser.Id);

            Words.Clear();
            foreach (var word in words)
            {
                Words.Add(word);
            }

            Categories.Clear();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }
        }

        private async Task ExecuteSearch()
        {
            try
            {
                IsLoading = true;
                var words = await _wordService.SearchWordsAsync(CurrentUser.Id, SearchText);
                Words.Clear();
                foreach (var word in words)
                {
                    Words.Add(word);
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Error searching words: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExecuteAddWord()
        {
            await _navigationService.NavigateToAddWordAsync();
        }

        private async Task ExecuteEditWord()
        {
            if (SelectedWord == null)
            {
                _notificationService.ShowError("Please select a word to edit");
                return;
            }

            await _navigationService.NavigateToEditWordAsync(SelectedWord.Id);
        }

        private async Task ExecuteDeleteWord()
        {
            if (SelectedWord == null)
            {
                _notificationService.ShowError("Please select a word to delete");
                return;
            }

            var result = await _notificationService.ShowConfirmationAsync(
                "Are you sure you want to delete this word?",
                "Delete Word"
            );

            if (result)
            {
                try
                {
                    IsLoading = true;
                    var success = await _wordService.DeleteWordAsync(SelectedWord.Id);
                    if (success)
                    {
                        Words.Remove(SelectedWord);
                        _notificationService.ShowSuccess("Word deleted successfully");
                    }
                    else
                    {
                        _notificationService.ShowError("Failed to delete word");
                    }
                }
                catch (Exception ex)
                {
                    _notificationService.ShowError($"Error deleting word: {ex.Message}");
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private async Task ExecuteCategoryChanged()
        {
            try
            {
                IsLoading = true;
                var words = await _wordService.GetWordsAsync(CurrentUser.Id, SelectedCategory);
                Words.Clear();
                foreach (var word in words)
                {
                    Words.Add(word);
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Error loading words by category: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
} 