using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Naviguard.WPF.ViewModels;
using System.Windows.Controls;

namespace Naviguard.WPF.Views.Groups
{
    public partial class GroupsPages : UserControl
    {
        public GroupsPages()
        {
            InitializeComponent();
        }
        public GroupsPages(GroupsPagesViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}
