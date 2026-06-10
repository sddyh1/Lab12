using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PhoneBookDI.Models;
using PhoneBookDI.Services;
using PhoneBookDI.ViewModels;
using System.Windows;

namespace PhoneBookDI
{
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();

            services.AddDbContext<PhoneBookDbEvseev2307aContext>(options =>
                options.UseSqlite("Data Source=PhoneBookDB_Evseev_2307a.db"));

            services.AddSingleton<IDialogService, DialogService>();
            services.AddTransient<ContactEditViewModel>();
            services.AddSingleton<ContactsListViewModel>();

            services.AddSingleton<Func<ContactEditViewModel>>(provider => () => provider.GetRequiredService<ContactEditViewModel>());

            services.AddSingleton<MainWindow>(provider =>
            {
                var window = new MainWindow();
                window.DataContext = provider.GetRequiredService<ContactsListViewModel>();
                return window;
            });

            _serviceProvider = services.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}