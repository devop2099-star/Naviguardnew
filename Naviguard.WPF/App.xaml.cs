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
    public partial class App : Application
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
                    // Registrar servicios por capas
                    services.AddInfrastructure(context.Configuration);
                    services.AddApplication();
                    services.AddPresentation();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            // Configurar CefSharp
            var settings = new CefSettings()
            {
                CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache"),
                LogSeverity = LogSeverity.Disable
            };
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);

            await _host.StartAsync();

            // Mostrar ventana de login (sin DI, porque necesita lógica especial)
            var loginWindow = new Login();
            loginWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
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