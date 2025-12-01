using Microsoft.Extensions.DependencyInjection;
using Naviguard.WPF.Services;
using Naviguard.WPF.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

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

            btnNav_Click(null, null);
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
            var groupsViewModel = _serviceProvider.GetRequiredService<GroupsPagesViewModel>();
            groupsViewModel.NavigateToGroupAction = NavigateToGroupView;

            var groupsView = _serviceProvider.GetRequiredService<GroupsPages>();
            groupsView.DataContext = groupsViewModel;

            _navigationService.NavigateTo(groupsView);
        }

        public void NavigateToGroupView(Domain.Entities.Group group)
        {
            var menuViewModel = ActivatorUtilities.CreateInstance<MenuNaviguardViewModel>(
                _serviceProvider,
                group.GroupId);

            var menuView = new Views.Browser.MenuNaviguardPages
            {
                DataContext = menuViewModel
            };

            _navigationService.NavigateTo(menuView);
        }

        private void btnFilterPages_Click(object sender, RoutedEventArgs e)
        {
            var view = _serviceProvider.GetRequiredService<FilterPagesNav>();
            _navigationService.NavigateTo(view);
        }

        private void btnEditGroups_Click(object sender, RoutedEventArgs e)
        {
            var view = _serviceProvider.GetRequiredService<EditGroups>();
            _navigationService.NavigateTo(view);
        }

        private void btnAssignUserToGroups_Click(object sender, RoutedEventArgs e)
        {
            var view = _serviceProvider.GetRequiredService<AssignUserToGroups>();
            _navigationService.NavigateTo(view);
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            UserSession.EndSession();
            Process.Start(Process.GetCurrentProcess().MainModule.FileName);
            Application.Current.Shutdown();
        }
    }
}