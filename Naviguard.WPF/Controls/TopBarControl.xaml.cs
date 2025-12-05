using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;

namespace Naviguard.WPF.Controls
{
    public partial class TopBarControl : UserControl
    {
        // ✅ API de Windows para multi-monitor
        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        private const uint MONITOR_DEFAULTTONEAREST = 2;

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
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.WindowState = WindowState.Minimized;
                Debug.WriteLine("➖ Ventana minimizada");
            }
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                if (window.WindowState == WindowState.Maximized)
                {
                    window.WindowState = WindowState.Normal;
                    Debug.WriteLine("🔽 Ventana restaurada desde TopBar");
                }
                else
                {
                    // ✅ Detectar el monitor actual antes de maximizar
                    IntPtr handle = new WindowInteropHelper(window).Handle;
                    IntPtr monitor = MonitorFromWindow(handle, MONITOR_DEFAULTTONEAREST);

                    if (monitor != IntPtr.Zero)
                    {
                        MONITORINFO monitorInfo = new MONITORINFO();
                        monitorInfo.cbSize = (uint)Marshal.SizeOf(typeof(MONITORINFO));

                        if (GetMonitorInfo(monitor, ref monitorInfo))
                        {
                            Debug.WriteLine($"🖥️ TopBar - Maximizando en monitor: " +
                                $"WorkArea=({monitorInfo.rcWork.Left},{monitorInfo.rcWork.Top})-" +
                                $"({monitorInfo.rcWork.Right},{monitorInfo.rcWork.Bottom})");
                        }
                    }

                    window.WindowState = WindowState.Maximized;
                    Debug.WriteLine("🔼 Ventana maximizada desde TopBar");
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.Close();
                Debug.WriteLine("❌ Ventana cerrada desde TopBar");
            }
        }
    }
}