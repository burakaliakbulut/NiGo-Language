using System.Windows;
using WordLearningWpfApp.ViewModels;

namespace WordLearningWpfApp.Views
{
    public partial class AddWordWindow : BaseWindow
    {
        public AddWordWindow(AddWordViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
} 