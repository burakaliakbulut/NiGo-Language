using System.Windows;
using WordLearningWpfApp.ViewModels;

namespace WordLearningWpfApp.Views
{
    public partial class DictionaryWindow : BaseWindow
    {
        public DictionaryWindow(DictionaryViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
} 