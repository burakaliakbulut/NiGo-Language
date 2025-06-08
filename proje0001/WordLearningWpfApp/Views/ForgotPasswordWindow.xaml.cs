using System.Windows;
using WordLearningWpfApp.ViewModels;

namespace WordLearningWpfApp.Views
{
    public partial class ForgotPasswordWindow : BaseWindow
    {
        public ForgotPasswordWindow(ForgotPasswordViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
} 