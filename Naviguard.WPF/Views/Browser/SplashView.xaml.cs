using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Naviguard.WPF.Views.Browser
{
    public partial class SplashView : UserControl
    {
        private readonly string targetText = "CARGANDO";
        private readonly string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private DispatcherTimer? timer; // ✅ Nullable
        private double iteration = 0;
        private Random rng = new Random();

        public SplashView()
        {
            InitializeComponent();
            this.Loaded += ShieldView_Loaded;
        }

        private void ShieldView_Loaded(object sender, RoutedEventArgs e)
        {
            ResetToWhite();
        }

        private void ResetToWhite()
        {
            BorderContainer.Background = Brushes.White;
            TxtCargando.Foreground = Brushes.Black;
            TxtCargando.Text = targetText;

            var delayTimer = new DispatcherTimer();
            delayTimer.Interval = TimeSpan.FromSeconds(0.5);
            delayTimer.Tick += (s, ev) =>
            {
                delayTimer.Stop();
                FadeToBlackAndScramble();
            };
            delayTimer.Start();
        }

        private void FadeToBlackAndScramble()
        {
            var bgAnim = new ColorAnimation
            {
                From = Colors.White,
                To = Colors.Black,
                Duration = TimeSpan.FromMilliseconds(500)
            };

            var brush = new SolidColorBrush(Colors.White);
            BorderContainer.Background = brush;
            brush.BeginAnimation(SolidColorBrush.ColorProperty, bgAnim);

            var fgAnim = new ColorAnimation
            {
                From = Colors.Black,
                To = Colors.White,
                Duration = TimeSpan.FromMilliseconds(500)
            };

            var fgBrush = new SolidColorBrush(Colors.Black);
            TxtCargando.Foreground = fgBrush;
            fgBrush.BeginAnimation(SolidColorBrush.ColorProperty, fgAnim);

            var startScrambleTimer = new DispatcherTimer();
            startScrambleTimer.Interval = TimeSpan.FromMilliseconds(500);
            startScrambleTimer.Tick += (s, e) =>
            {
                startScrambleTimer.Stop();
                StartScrambleEffect();
            };
            startScrambleTimer.Start();
        }

        private void StartScrambleEffect()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(30);
            timer.Tick += Timer_Tick;
            iteration = 0;
            timer.Start();
        }

        // ✅ Corregido - parámetros nullable
        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (timer == null) return; // ✅ Validación

            if (iteration >= targetText.Length)
            {
                timer.Stop();
                TxtCargando.Text = targetText;

                var restartTimer = new DispatcherTimer();
                restartTimer.Interval = TimeSpan.FromSeconds(1);
                restartTimer.Tick += (s2, e2) =>
                {
                    restartTimer.Stop();
                    FadeBackToWhite();
                };
                restartTimer.Start();

                return;
            }

            var current = targetText
                .Select((ch, idx) =>
                {
                    if (idx < iteration)
                        return targetText[idx];
                    else
                        return letters[rng.Next(letters.Length)];
                })
                .ToArray();

            TxtCargando.Text = new string(current);

            iteration += 1.0 / 5;
        }

        private void FadeBackToWhite()
        {
            var bgAnim = new ColorAnimation
            {
                From = Colors.Black,
                To = Colors.White,
                Duration = TimeSpan.FromMilliseconds(500)
            };

            var brush = new SolidColorBrush(Colors.Black);
            BorderContainer.Background = brush;
            brush.BeginAnimation(SolidColorBrush.ColorProperty, bgAnim);

            var fgAnim = new ColorAnimation
            {
                From = Colors.White,
                To = Colors.Black,
                Duration = TimeSpan.FromMilliseconds(500)
            };

            var fgBrush = new SolidColorBrush(Colors.White);
            TxtCargando.Foreground = fgBrush;
            fgBrush.BeginAnimation(SolidColorBrush.ColorProperty, fgAnim);

            var restartTimer = new DispatcherTimer();
            restartTimer.Interval = TimeSpan.FromMilliseconds(500);
            restartTimer.Tick += (s, e) =>
            {
                restartTimer.Stop();
                ResetToWhite();
            };
            restartTimer.Start();
        }
    }
}