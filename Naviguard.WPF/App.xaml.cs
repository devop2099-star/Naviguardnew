using CefSharp;
using CefSharp.Wpf;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Naviguard.WPF.DependencyInjection;
using Naviguard.WPF.Views.Login;
using System.IO;
using System.Windows;

namespace Naviguard.WPF
{
    public partial class App : System.Windows.Application
    {
        private readonly IHost _host;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddInfrastructure(context.Configuration);
                    services.AddApplication();
                    services.AddPresentation();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            // ✅ Configuración Minimalista (Idéntica a Legacy, estable)
            var settings = new CefSettings()
            {
                CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache"),
                LogSeverity = LogSeverity.Disable
            };

            // ✅ Inicializar CefSharp con configuración simple
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);

            await _host.StartAsync();

            var loginWindow = new Login();
            loginWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            // ✅ Apagar CefSharp de forma ordenada
            Cef.Shutdown();

            using (_host)
            {
                await _host.StopAsync();
            }

            base.OnExit(e);
        }

        public static T GetService<T>() where T : class
        {
            return ((App)Current)._host.Services.GetRequiredService<T>();
        }
    }
}