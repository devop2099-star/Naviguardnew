using CefSharp.Wpf;
using Naviguard.Domain.Entities;
using Naviguard.WPF.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Naviguard.WPF.Views.Browser
{
    public partial class BrowserView : UserControl
    {
        private BrowserViewModel? _viewModel;

        public BrowserView()
        {
            InitializeComponent();
            Debug.WriteLine("[BrowserView] Constructor llamado");
        }

        public async Task InitializeAsync(BrowserViewModel viewModel, Pagina page)
        {
            Debug.WriteLine($"[BrowserView] InitializeAsync llamado para: {page.PageName}");

            _viewModel = viewModel;
            DataContext = viewModel;

            if (BrowserControl is ChromiumWebBrowser browser)
            {
                Debug.WriteLine($"[BrowserView] ChromiumWebBrowser encontrado, inicializando...");
                await viewModel.InitializeBrowserAsync(browser, page);
            }
            else
            {
                Debug.WriteLine($"[BrowserView] ERROR: ChromiumWebBrowser NO encontrado");
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[BrowserView] Unloaded event");
            _viewModel?.Cleanup();
        }
    }
}