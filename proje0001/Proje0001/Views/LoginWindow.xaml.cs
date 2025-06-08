using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Proje0001.Views
{
    public partial class LoginWindow : Window
    {
        private TextBlock ErrorTextBlock;
        private TextBox EmailTextBox;
        private PasswordBox PasswordTextBox;
        private ProgressBar LoadingIndicator;

        public LoginWindow()
        {
            InitializeComponent();
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            ErrorTextBlock = FindName("ErrorTextBlock") as TextBlock;
            EmailTextBox = FindName("EmailTextBox") as TextBox;
            PasswordTextBox = FindName("PasswordTextBox") as PasswordBox;
            LoadingIndicator = FindName("LoadingIndicator") as ProgressBar;
        }

        private void ShowError(string message)
        {
            if (ErrorTextBlock != null)
            {
                ErrorTextBlock.Text = message;
                ErrorTextBlock.Visibility = Visibility.Visible;
            }
        }

        private void ClearError()
        {
            if (ErrorTextBlock != null)
            {
                ErrorTextBlock.Text = string.Empty;
                ErrorTextBlock.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowLoading()
        {
            if (LoadingIndicator != null)
            {
                LoadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HideLoading()
        {
            if (LoadingIndicator != null)
            {
                LoadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
    }
} 