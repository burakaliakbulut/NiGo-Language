using System.Windows;
using WordLearningWpfApp.ViewModels;

namespace WordLearningWpfApp.Views
{
    public partial class WordListWindow : BaseWindow
    {
        public WordListWindow(WordListViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
} 