using System.Windows;
using WordLearningWpfApp.ViewModels;

namespace WordLearningWpfApp.Views
{
    public partial class DailyWordsWindow : BaseWindow
    {
        public DailyWordsWindow(DailyWordsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
} 