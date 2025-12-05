using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Naviguard.WPF.Controls
{
    public partial class TopBarControl : UserControl
    {
        public TopBarControl()
        {
            InitializeComponent();
        }

        private void TopBar_MouseEnter(object sender, MouseEventArgs e)
        {
            var expand = (Storyboard)FindResource("ExpandStoryboard");
            expand.Begin();
        }

        private void TopBar_MouseLeave(object sender, MouseEventArgs e)
        {
            var collapse = (Storyboard)FindResource("CollapseStoryboard");
            collapse.Begin();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.WindowState = window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }
    }
}
