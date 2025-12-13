using Naviguard.WPF.ViewModels;
using System.Windows.Controls;

namespace Naviguard.WPF.Views.Credentials
{
    public partial class EditCustomLoginView : UserControl
    {
        public EditCustomLoginView()
        {
            InitializeComponent();
        }

        public EditCustomLoginView(EditCustomLoginViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}
