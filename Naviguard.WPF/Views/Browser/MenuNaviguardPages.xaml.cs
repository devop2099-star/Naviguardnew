using Naviguard.Domain.Entities;
using System.Windows.Controls;
using Naviguard.WPF.ViewModels;

namespace Naviguard.WPF.Views.Browser
{
    public partial class MenuNaviguardPages : UserControl
    {
        public MenuNaviguardPages()
        {
            InitializeComponent();
        }
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is Pagina selectedPage)
            {
                if (DataContext is MenuNaviguardViewModel viewModel)
                {
                    viewModel.OpenPageCommand.Execute(selectedPage);
                }
            }
        }
    }
}
