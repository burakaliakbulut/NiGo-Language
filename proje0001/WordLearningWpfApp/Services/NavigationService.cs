using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WordLearningWpfApp.Interfaces;
using WordLearningWpfApp.Models;
using WordLearningWpfApp.ViewModels;
using WordLearningWpfApp.Views;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Collections.Generic;

namespace WordLearningWpfApp.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private Window? _currentWindow;
        private object _currentViewModel;
        private List<object> _navigationHistory;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _navigationHistory = [];
        }

        public bool CanGoBack => _navigationHistory.Count > 0;
        public bool CanGoForward => false;

        public object CurrentViewModel => _currentViewModel ?? throw new InvalidOperationException("No view model is currently set.");

        public event EventHandler<object> CurrentViewModelChanged;

        public void NavigateTo<TViewModel>() where TViewModel : class
        {
            var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
            NavigateTo(viewModel);
        }

        public void NavigateTo(object viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            if (_currentViewModel != null)
            {
                _navigationHistory.Add(_currentViewModel);
            }
            _currentViewModel = viewModel;
            CurrentViewModelChanged?.Invoke(this, viewModel);
        }

        public void GoBack()
        {
            if (!CanGoBack)
                throw new InvalidOperationException("Cannot go back, no previous view model exists.");

            var previousViewModel = _navigationHistory[_navigationHistory.Count - 1];
            _navigationHistory.RemoveAt(_navigationHistory.Count - 1);
            _currentViewModel = previousViewModel;
            CurrentViewModelChanged?.Invoke(this, previousViewModel);
        }

        public void GoForward()
        {
            throw new NotImplementedException("Forward navigation is not supported.");
        }

        public void ShowWindow<TWindow>() where TWindow : Window
        {
            var window = _serviceProvider.GetRequiredService<TWindow>();
            _currentWindow = window;
            window.Show();
        }

        public void ShowDialog<TWindow>() where TWindow : Window
        {
            var window = _serviceProvider.GetRequiredService<TWindow>();
            _currentWindow = window;
            window.ShowDialog();
        }

        public void CloseCurrentWindow()
        {
            _currentWindow?.Close();
            _currentWindow = null;
        }

        public void Initialize(Window mainWindow)
        {
            _currentWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        }

        public void NavigateTo(string viewName)
        {
            NavigateToAsync(viewName).GetAwaiter().GetResult();
        }

        public async Task NavigateToAsync(string pageName, object? parameter = null)
        {
            try
            {
                var viewModel = CreateViewModel(pageName, parameter);
                var window = CreateWindowForViewModel(viewModel);

                if (viewModel is IInitializable initializable)
                {
                    await initializable.InitializeAsync();
                }

                _currentWindow?.Close();

                _currentWindow = window;
                window.Show();
            }
            catch (Exception ex)
            {
                throw new Exception($"Navigation failed: {ex.Message}", ex);
            }
        }

        private object CreateViewModel(string pageName, object parameter)
        {
            return pageName switch
            {
                "Login" => _serviceProvider.GetRequiredService<LoginViewModel>(),
                "Register" => _serviceProvider.GetRequiredService<RegisterViewModel>(),
                "Main" => _serviceProvider.GetRequiredService<MainViewModel>(),
                "Dictionary" => _serviceProvider.GetRequiredService<DictionaryViewModel>(),
                "AddWord" => _serviceProvider.GetRequiredService<AddWordViewModel>(),
                "EditWord" => CreateEditWordViewModel(parameter),
                "Settings" => _serviceProvider.GetRequiredService<SettingsViewModel>(),
                "Statistics" => _serviceProvider.GetRequiredService<StatisticsViewModel>(),
                "Progress" => _serviceProvider.GetRequiredService<ProgressViewModel>(),
                "Exam" => _serviceProvider.GetRequiredService<ExamViewModel>(),
                _ => throw new ArgumentException($"Unknown page: {pageName}")
            };
        }

        private object CreateEditWordViewModel(object? parameter)
        {
            var viewModel = _serviceProvider.GetRequiredService<EditWordViewModel>();
            if (parameter is string wordId)
            {
                viewModel.Initialize(wordId);
            }
            return viewModel;
        }

        private Window CreateWindowForViewModel(object viewModel)
        {
            return viewModel switch
            {
                LoginViewModel _ => new LoginWindow(
                    _serviceProvider.GetRequiredService<IAuthService>(),
                    this,
                    _serviceProvider.GetRequiredService<INotificationService>(),
                    _serviceProvider.GetRequiredService<IWordService>(),
                    _serviceProvider.GetRequiredService<IStatisticsService>(),
                    _serviceProvider.GetRequiredService<IExamService>()),
                RegisterViewModel _ => new RegisterWindow(
                    _serviceProvider.GetRequiredService<IAuthService>(),
                    this,
                    _serviceProvider.GetRequiredService<INotificationService>()),
                MainViewModel _ => new MainWindow(viewModel),
                DictionaryViewModel _ => new DictionaryWindow(viewModel),
                AddWordViewModel _ => new AddWordWindow(viewModel),
                EditWordViewModel _ => new EditWordWindow(viewModel),
                SettingsViewModel _ => new SettingsWindow(viewModel),
                StatisticsViewModel _ => new StatisticsWindow(viewModel),
                ProgressViewModel _ => new ProgressWindow(viewModel),
                ExamViewModel _ => new ExamWindow(viewModel),
                _ => throw new ArgumentException($"Unknown view model type: {viewModel.GetType()}")
            };
        }

        public async Task GoBackAsync()
        {
            if (_currentWindow == null)
                throw new InvalidOperationException("Main window is not initialized");

            if (_currentWindow.Owner != null)
            {
                _currentWindow.Owner.Show();
                _currentWindow.Close();
                _currentWindow = _currentWindow.Owner;
            }
            else
            {
                await NavigateToAsync("Login");
            }
        }

        public async Task NavigateToLoginAsync()
        {
            await NavigateToAsync("Login");
        }

        public async Task NavigateToRegisterAsync()
        {
            await NavigateToAsync("Register");
        }

        public async Task NavigateToMain()
        {
            await NavigateToAsync("Main");
        }

        public async Task NavigateToWordListAsync()
        {
            await NavigateToAsync("WordList");
        }

        public async Task NavigateToExamAsync()
        {
            await NavigateToAsync("Exam");
        }

        public async Task NavigateToProgressAsync()
        {
            await NavigateToAsync("Progress");
        }

        public async Task NavigateToSettingsAsync()
        {
            await NavigateToAsync("Settings");
        }

        public async Task NavigateToWordStatsAsync()
        {
            await NavigateToAsync("WordStats");
        }

        public async Task NavigateToForgotPasswordAsync()
        {
            await NavigateToAsync("ForgotPassword");
        }

        public async Task NavigateBackAsync()
        {
            await GoBackAsync();
        }

        public async Task NavigateToDailyWordsAsync()
        {
            await NavigateToAsync("WordList");
        }

        public async Task NavigateToStatisticsAsync()
        {
            await NavigateToAsync("WordStats");
        }

        public async Task NavigateToDictionaryAsync()
        {
            await NavigateToAsync("Dictionary");
        }

        public async Task NavigateToMainAsync()
        {
            await NavigateToAsync("Main");
        }

        public async Task NavigateToAsync(string pageName)
        {
            await NavigateToAsync(pageName, null);
        }

        public async Task NavigateToAddWordAsync()
        {
            await NavigateToAsync("AddWord");
        }

        public async Task NavigateToEditWordAsync(string? wordId)
        {
            await NavigateToAsync("EditWord", wordId);
        }

        public async Task NavigateToEditWordAsync(Word word)
        {
            await NavigateToAsync("EditWord", word.Id);
        }

        public async Task NavigateToWordStatsAsync(Word word)
        {
            await NavigateToAsync("WordStats", word.Id);
        }

        public async Task NavigateToLogin()
        {
            await NavigateToAsync("Login");
        }

        public void ShowMessage(string? message, string? title = "Information")
        {
            MessageBox.Show(_currentWindow, message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public bool ShowConfirmation(string? message, string? title = "Confirmation")
        {
            return MessageBox.Show(_currentWindow, message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
    }
} 