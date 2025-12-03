// Naviguard.WPF/Views/Browser/MenuNaviguardPages.xaml.cs
using System.Windows.Controls;
using System.Windows;

namespace Naviguard.WPF.Views.Browser
{
    public partial class MenuNaviguardPages : UserControl
    {
        public MenuNaviguardPages()
        {
            InitializeComponent();
        }

        private void OpenPagesMenu_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.IsOpen = true;
            }
        }
    }
}