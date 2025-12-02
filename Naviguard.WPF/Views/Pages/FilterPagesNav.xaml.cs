using Naviguard.WPF.ViewModels;
using System.Windows.Controls;

namespace Naviguard.WPF.Views.Pages
{
    public partial class FilterPagesNav : UserControl
    {
        public FilterPagesNav()
        {
            InitializeComponent();
        }
        public FilterPagesNav(FilterPagesViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}
