using System.Windows;
using WordLearningWpfApp.ViewModels;

namespace WordLearningWpfApp.Views
{
    public partial class ProgressWindow : BaseWindow
    {
        public ProgressWindow(ProgressViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
} 