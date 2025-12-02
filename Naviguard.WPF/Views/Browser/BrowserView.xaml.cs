using CefSharp.Wpf;
using Naviguard.Domain.Entities;
using Naviguard.WPF.ViewModels;
using System.Windows.Controls;

namespace Naviguard.WPF.Views.Browser
{
    public partial class BrowserView : UserControl
    {
        private BrowserViewModel? _viewModel;
        public BrowserView()
        {
            InitializeComponent();
        }
        public async Task InitializeAsync(BrowserViewModel viewModel, Pagina page)
        {
            _viewModel = viewModel;
            DataContext = viewModel;

            // BrowserControl debe ser el nombre del ChromiumWebBrowser en tu XAML
            if (Browser is ChromiumWebBrowser browser)
            {
                await viewModel.InitializeBrowserAsync(browser, page);
            }
        }

        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _viewModel?.Cleanup();
        }
    }
}
