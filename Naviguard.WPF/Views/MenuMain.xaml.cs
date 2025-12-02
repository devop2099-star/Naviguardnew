using Microsoft.Extensions.DependencyInjection;
using Naviguard.Domain.Entities;
using Naviguard.WPF.Services;
using Naviguard.WPF.ViewModels;
using Naviguard.WPF.Views.Browser;
using Naviguard.WPF.Views.Groups;
using Naviguard.WPF.Views.Pages;
using Naviguard.WPF.Views.Users;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using WpfApp = System.Windows.Application;

namespace Naviguard.WPF.Views
{
    public partial class MenuMain : Window
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly NavigationService _navigationService;
        private bool _hasAdminAccess;

        public MenuMain(IServiceProvider serviceProvider, NavigationService navigationService)
        {
            InitializeComponent();

            _serviceProvider = serviceProvider;
            _navigationService = navigationService;

            _navigationService.Initialize(ContentPresenter);
        }

        public void SetAdminAccess(bool hasAdminAccess)
        {
            _hasAdminAccess = hasAdminAccess;

            var adminVisibility = hasAdminAccess ? Visibility.Visible : Visibility.Collapsed;
            btnFilterPages.Visibility = adminVisibility;
            btnEditGroups.Visibility = adminVisibility;
            btnAssignUserToGroups.Visibility = adminVisibility;

            // ✅ Corregido - usar objeto anónimo en lugar de null
            btnNav_Click(this, new RoutedEventArgs());
        }

        private void MainBorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void btnNav_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("🔄 Navegando a vista de grupos...");

                var groupsViewModel = _serviceProvider.GetRequiredService<GroupsPagesViewModel>();

                if (groupsViewModel == null)
                {
                    MessageBox.Show("No se pudo cargar el ViewModel de grupos", "Error");
                    return;
                }

                groupsViewModel.NavigateToGroupAction = NavigateToGroupView;

                var groupsView = _serviceProvider.GetRequiredService<GroupsPages>();

                if (groupsView == null)
                {
                    MessageBox.Show("No se pudo cargar la vista de grupos", "Error");
                    return;
                }

                groupsView.DataContext = groupsViewModel;
                _navigationService.NavigateTo(groupsView);

                Debug.WriteLine("✅ Vista de grupos cargada correctamente");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error en btnNav_Click: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                MessageBox.Show($"Error al cargar grupos: {ex.Message}", "Error");
            }
        }

        public void NavigateToGroupView(Group group)
        {
            var menuViewModel = ActivatorUtilities.CreateInstance<MenuNaviguardViewModel>(
                _serviceProvider,
                group.GroupId);

            menuViewModel.OpenPageAction = OpenPageInBrowser;

            var menuView = new MenuNaviguardPages
            {
                DataContext = menuViewModel
            };

            _navigationService.NavigateTo(menuView);
        }

        private async void OpenPageInBrowser(Pagina page)
        {
            try
            {
                Debug.WriteLine($"Abriendo página en navegador: {page.PageName}");

                var browserViewModel = _serviceProvider.GetRequiredService<BrowserViewModel>();
                var browserView = new BrowserView();

                _navigationService.NavigateTo(browserView);

                await browserView.InitializeAsync(browserViewModel, page);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir navegador: {ex.Message}", "Error");
                Debug.WriteLine($"Error: {ex}");
            }
        }

        private void btnFilterPages_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = _serviceProvider.GetRequiredService<FilterPagesViewModel>();
            var view = _serviceProvider.GetRequiredService<FilterPagesNav>();
            view.DataContext = viewModel;
            _navigationService.NavigateTo(view);
        }

        private void btnEditGroups_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = _serviceProvider.GetRequiredService<EditGroupsViewModel>();
            var view = _serviceProvider.GetRequiredService<EditGroups>();
            view.DataContext = viewModel;
            _navigationService.NavigateTo(view);
        }

        private void btnAssignUserToGroups_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = _serviceProvider.GetRequiredService<AssignUserToGroupsViewModel>();
            var view = _serviceProvider.GetRequiredService<AssignUserToGroups>();
            view.DataContext = viewModel;
            _navigationService.NavigateTo(view);
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            UserSession.EndSession();
            Process.Start(Process.GetCurrentProcess().MainModule!.FileName!);
            WpfApp.Current.Shutdown();
        }
    }
}