// Naviguard.WPF/Views/MenuMain.xaml.cs
using Microsoft.Extensions.DependencyInjection;
using Naviguard.Domain.Entities;
using Naviguard.WPF.Services;
using Naviguard.WPF.ViewModels;
using Naviguard.WPF.Views.Browser;
using Naviguard.WPF.Views.Groups;
using Naviguard.WPF.Views.Pages;
using Naviguard.WPF.Views.Users;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using WpfApp = System.Windows.Application;
// ✅ NO importar el namespace Login para evitar conflictos

namespace Naviguard.WPF.Views
{
    public partial class MenuMain : Window
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly NavigationService _navigationService;
        private bool _hasAdminAccess;
        private MenuNaviguardViewModel? _currentMenuViewModel;

        // ✅ Para controlar el tamaño máximo respetando la barra de tareas y multi-monitor
        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        private const uint MONITOR_DEFAULTTONEAREST = 2;
        private const uint MONITOR_DEFAULTTOPRIMARY = 1;

        [StructLayout(LayoutKind.Sequential)]
        public struct MONITORINFO
        {
            public uint cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public MenuMain(IServiceProvider serviceProvider, NavigationService navigationService)
        {
            InitializeComponent();

            _serviceProvider = serviceProvider;
            _navigationService = navigationService;

            _navigationService.Initialize(ContentPresenter);

            // ✅ Suscribirse al evento de carga para configurar el comportamiento de maximizado
            this.Loaded += MenuMain_Loaded;
            this.StateChanged += MenuMain_StateChanged;
        }

        private void MenuMain_Loaded(object sender, RoutedEventArgs e)
        {
            // ✅ Configurar el comportamiento al maximizar
            IntPtr handle = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(handle)?.AddHook(WindowProc);
        }

        private void MenuMain_StateChanged(object? sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                // ✅ Cuando está maximizada: sin bordes redondeados ni márgenes extra
                this.BorderThickness = new Thickness(0);
                MainBorder.CornerRadius = new CornerRadius(0);
                MainBorder.Margin = new Thickness(0);
            }
            else
            {
                // ✅ Restaurar cuando está normal
                this.BorderThickness = new Thickness(0);
                MainBorder.CornerRadius = new CornerRadius(30);
                MainBorder.Margin = new Thickness(0);
            }
        }

        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_GETMINMAXINFO = 0x0024;

            if (msg == WM_GETMINMAXINFO)
            {
                // ✅ Obtener el monitor donde está actualmente la ventana
                IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

                if (monitor != IntPtr.Zero)
                {
                    MONITORINFO monitorInfo = new MONITORINFO();
                    monitorInfo.cbSize = (uint)Marshal.SizeOf(typeof(MONITORINFO));

                    if (GetMonitorInfo(monitor, ref monitorInfo))
                    {
                        // ✅ rcWork es el área de trabajo (excluye barra de tareas)
                        RECT workArea = monitorInfo.rcWork;
                        // ✅ rcMonitor es el área total del monitor
                        RECT monitorArea = monitorInfo.rcMonitor;

                        MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO))!;

                        // ✅ Configurar posición máxima relativa al monitor actual
                        mmi.ptMaxPosition.X = workArea.Left - monitorArea.Left;
                        mmi.ptMaxPosition.Y = workArea.Top - monitorArea.Top;

                        // ✅ Configurar tamaño máximo respetando la barra de tareas
                        mmi.ptMaxSize.X = workArea.Right - workArea.Left;
                        mmi.ptMaxSize.Y = workArea.Bottom - workArea.Top;

                        // ✅ Configurar tamaño mínimo (opcional)
                        mmi.ptMinTrackSize.X = (int)this.MinWidth;
                        mmi.ptMinTrackSize.Y = (int)this.MinHeight;

                        Marshal.StructureToPtr(mmi, lParam, true);

                    }
                }

                handled = true;
            }

            return IntPtr.Zero;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        // ✅ AGREGAR: Permitir mover la ventana haciendo clic en el MainBorder
        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Solo permitir mover si no está maximizada
            if (this.WindowState != WindowState.Maximized && e.ClickCount == 1)
            {
                this.DragMove();
            }
            // Doble clic para maximizar/restaurar
            else if (e.ClickCount == 2)
            {
                ToggleMaximize();
            }
            // ✅ Si arrastra mientras está maximizada, restaurar y mover
            else if (this.WindowState == WindowState.Maximized && e.ClickCount == 1)
            {
                // Obtener la posición del mouse antes de restaurar
                var mousePos = e.GetPosition(this);
                var screenPos = PointToScreen(mousePos);

                // Restaurar ventana
                this.WindowState = WindowState.Normal;

                // Posicionar la ventana centrada bajo el cursor
                this.Left = screenPos.X - (this.ActualWidth / 2);
                this.Top = screenPos.Y - 20; // Un poco debajo del cursor

                // Iniciar el arrastre
                this.DragMove();
            }
        }

        // ✅ Método auxiliar para maximizar/restaurar
        private void ToggleMaximize()
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                Debug.WriteLine("🔽 Ventana restaurada");
            }
            else
            {
                // ✅ Antes de maximizar, asegurar que se detecte el monitor correcto
                IntPtr handle = new WindowInteropHelper(this).Handle;
                IntPtr monitor = MonitorFromWindow(handle, MONITOR_DEFAULTTONEAREST);

                if (monitor != IntPtr.Zero)
                {
                    MONITORINFO monitorInfo = new MONITORINFO();
                    monitorInfo.cbSize = (uint)Marshal.SizeOf(typeof(MONITORINFO));

                    if (GetMonitorInfo(monitor, ref monitorInfo))
                    {
                        Debug.WriteLine($"🖥️ Maximizando en monitor: Left={monitorInfo.rcWork.Left}, Top={monitorInfo.rcWork.Top}");
                    }
                }

                this.WindowState = WindowState.Maximized;
                Debug.WriteLine("🔼 Ventana maximizada");
            }
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

        private void btnNav_Click(object sender, RoutedEventArgs e)
        {
            _currentMenuViewModel = null;

            var groupsViewModel = _serviceProvider.GetRequiredService<GroupsPagesViewModel>();
            groupsViewModel.NavigateToGroupAction = NavigateToGroupView;

            var groupsView = _serviceProvider.GetRequiredService<GroupsPages>();
            groupsView.DataContext = groupsViewModel;

            _navigationService.NavigateTo(groupsView);
        }

        public void NavigateToGroupView(Group group)
        {
            var menuViewModel = ActivatorUtilities.CreateInstance<MenuNaviguardViewModel>(
                _serviceProvider,
                group.GroupId);

            menuViewModel.OpenPageAction = OpenPageInBrowser;
            _currentMenuViewModel = menuViewModel;

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

                await browserView.InitializeAsync(browserViewModel, page);
                _currentMenuViewModel?.RegisterBrowserView(page, browserView);

                Debug.WriteLine("Vista del navegador registrada correctamente");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir navegador: {ex.Message}", "Error");
                Debug.WriteLine($"Error: {ex}");
            }
        }

        private void btnFilterPages_Click(object sender, RoutedEventArgs e)
        {
            _currentMenuViewModel = null;

            var viewModel = _serviceProvider.GetRequiredService<FilterPagesViewModel>();
            var view = _serviceProvider.GetRequiredService<FilterPagesNav>();
            view.DataContext = viewModel;
            _navigationService.NavigateTo(view);
        }

        private void btnEditGroups_Click(object sender, RoutedEventArgs e)
        {
            _currentMenuViewModel = null;

            var viewModel = _serviceProvider.GetRequiredService<EditGroupsViewModel>();
            var view = _serviceProvider.GetRequiredService<EditGroups>();
            view.DataContext = viewModel;
            _navigationService.NavigateTo(view);
        }

        private void btnAssignUserToGroups_Click(object sender, RoutedEventArgs e)
        {
            _currentMenuViewModel = null;

            var viewModel = _serviceProvider.GetRequiredService<AssignUserToGroupsViewModel>();
            var view = _serviceProvider.GetRequiredService<AssignUserToGroups>();
            view.DataContext = viewModel;
            _navigationService.NavigateTo(view);
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("🔚 Cerrando sesión...");

                UserSession.EndSession();
                this.Hide();

                var loginWindow = new Naviguard.WPF.Views.Login.Login();

                EventHandler? closedHandler = null;
                closedHandler = (s, args) =>
                {
                    Debug.WriteLine("⚠️ Login cerrado sin iniciar sesión");
                    WpfApp.Current.Shutdown();
                };

                loginWindow.Closed += closedHandler;
                loginWindow.Tag = closedHandler;

                loginWindow.Show();
                this.Close();

                Debug.WriteLine("✅ Logout completado");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error en logout: {ex.Message}");
                MessageBox.Show($"Error al cerrar sesión: {ex.Message}", "Error");
            }
        }
    }
}