using Naviguard.WPF.ViewModels;
using System.Windows.Controls;

namespace Naviguard.WPF.Views.Groups
{
    public partial class EditGroups : UserControl
    {
        public EditGroups()
        {
            InitializeComponent();
        }
        public EditGroups(EditGroupsViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}
