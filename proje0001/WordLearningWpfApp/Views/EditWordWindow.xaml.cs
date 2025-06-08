using System.Windows;
using WordLearningWpfApp.ViewModels;

namespace WordLearningWpfApp.Views
{
    public partial class EditWordWindow : BaseWindow
    {
        public EditWordWindow(EditWordViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
} 