using System;
using System.Threading.Tasks;
using System.Windows;
using Proje0001.Services;
using Proje0001.ViewModels;
using Proje0001.Views;

namespace Proje0001.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private Window _mainWindow;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public void Initialize(Window mainWindow)
        {
            _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        }

        public async Task NavigateToAsync(string viewName, object parameter = null)
        {
            try
            {
                if (_mainWindow == null)
                    throw new InvalidOperationException("Navigation service not initialized. Call Initialize first.");

                Window window = null;
                switch (viewName)
                {
                    case "Login":
                        window = new LoginView();
                        break;
                    case "Register":
                        window = new RegisterView();
                        break;
                    case "Main":
                        window = new MainView();
                        break;
                    case "Dictionary":
                        window = new DictionaryView();
                        break;
                    case "Statistics":
                        window = new StatisticsView();
                        break;
                    case "Progress":
                        window = new ProgressView();
                        break;
                    case "Settings":
                        window = new SettingsView();
                        break;
                    default:
                        throw new ArgumentException($"Unknown view: {viewName}", nameof(viewName));
                }

                if (window.DataContext is IInitializable initializable)
                {
                    await initializable.InitializeAsync(parameter);
                }

                _mainWindow.Close();
                _mainWindow = window;
                window.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Navigation error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        public async Task GoBackAsync()
        {
            try
            {
                if (_mainWindow == null)
                    throw new InvalidOperationException("Navigation service not initialized. Call Initialize first.");

                if (_mainWindow.Owner != null)
                {
                    _mainWindow.Close();
                    _mainWindow = _mainWindow.Owner;
                    _mainWindow.Show();
                }
                else
                {
                    await NavigateToAsync("Login");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Navigation error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        public void ShowMessage(string message, string title = "Information", MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.Information)
        {
            MessageBox.Show(message, title, buttons, icon);
        }

        public bool ShowConfirmation(string message, string title = "Confirmation")
        {
            return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
    }
} 