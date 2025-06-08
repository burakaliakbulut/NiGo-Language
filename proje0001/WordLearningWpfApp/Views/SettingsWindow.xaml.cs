using System.Windows;
using WordLearningWpfApp.ViewModels;

namespace WordLearningWpfApp.Views
{
    public partial class SettingsWindow : BaseWindow
    {
        public SettingsWindow(SettingsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
} 