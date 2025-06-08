using System.Windows;
using WordLearningWpfApp.ViewModels;

namespace WordLearningWpfApp.Views
{
    public partial class WordStatsWindow : BaseWindow
    {
        public WordStatsWindow(WordStatsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
} 