using Naviguard.WPF.ViewModels;
using System.Windows.Controls;

namespace Naviguard.WPF.Views.Users
{
    public partial class AssignUserToGroups : UserControl
    {
        public AssignUserToGroups()
        {
            InitializeComponent();
        }
        public AssignUserToGroups(AssignUserToGroupsViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}
