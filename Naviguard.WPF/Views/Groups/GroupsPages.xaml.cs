using System.Windows;
using System.Windows.Controls;
using Naviguard.WPF.ViewModels;

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

        // ✅ Método que faltaba
        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.IsOpen = true;
            }
        }
    }
}