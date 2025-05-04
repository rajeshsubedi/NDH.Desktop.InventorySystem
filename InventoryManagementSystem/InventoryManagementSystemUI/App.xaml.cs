using System.Configuration;
using System.Data;
using System.Windows;
using InventoryManagementSystemUI.Login;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryManagementSystemUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            RegisterAllServices();
            base.OnStartup(e);
            var loginWindow = new LoginDashboard();
            loginWindow.Show();
        }

        public void RegisterAllServices()
        {
            var services = new ServiceCollection();
            // Register services
            services.RegisterServices();

            // Add configuration if needed
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            services.AddSingleton<IConfiguration>(configuration);

            ServiceProvider = services.BuildServiceProvider();
        }
    }

}
