using System;
using System.Threading.Tasks;
using System.Windows;
using MaterialDesignThemes.Wpf;

namespace WordLearningWpfApp.Services
{
    public class ThemeService : IThemeService
    {
        private readonly PaletteHelper _paletteHelper;
        private bool _isDarkTheme;
        private string _currentTheme = "Light";
        private bool _isDarkMode;

        public ThemeService()
        {
            _paletteHelper = new PaletteHelper();
            _isDarkTheme = false;
        }

        public bool IsDarkTheme => _isDarkTheme;

        public async Task SetThemeAsync(string themeName)
        {
            _currentTheme = themeName;
            await ApplyThemeAsync(themeName);
        }

        public async Task<string> GetCurrentThemeAsync()
        {
            return _currentTheme;
        }

        public async Task<bool> IsDarkModeEnabledAsync()
        {
            return _isDarkTheme;
        }

        public async Task ToggleDarkModeAsync()
        {
            _isDarkTheme = !_isDarkTheme;
            await ApplyThemeAsync(_isDarkTheme ? "Dark" : "Light");
        }

        public async Task ApplyThemeAsync(string themeName)
        {
            var app = Application.Current;
            if (app == null) return;

            var themeUri = new Uri($"/Themes/{themeName}Theme.xaml", UriKind.Relative);
            var themeResource = new ResourceDictionary { Source = themeUri };

            app.Resources.MergedDictionaries.Clear();
            app.Resources.MergedDictionaries.Add(themeResource);
        }

        public void ToggleTheme()
        {
            _isDarkTheme = !_isDarkTheme;
            var theme = _paletteHelper.GetTheme();
            theme.SetBaseTheme(_isDarkTheme ? Theme.Dark : Theme.Light);
            _paletteHelper.SetTheme(theme);
        }

        public void SetTheme(bool isDark)
        {
            if (_isDarkTheme != isDark)
            {
                _isDarkTheme = isDark;
                ApplyTheme();
            }
        }

        private void ApplyTheme()
        {
            var theme = _paletteHelper.GetTheme();
            theme.SetBaseTheme(_isDarkTheme ? Theme.Dark : Theme.Light);
            _paletteHelper.SetTheme(theme);
        }

        public void SetPrimaryColor(string color)
        {
            var theme = _paletteHelper.GetTheme();
            theme.SetPrimaryColor((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color));
            _paletteHelper.SetTheme(theme);
        }

        public void SetAccentColor(string color)
        {
            var theme = _paletteHelper.GetTheme();
            theme.SetSecondaryColor((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color));
            _paletteHelper.SetTheme(theme);
        }
    }
} 