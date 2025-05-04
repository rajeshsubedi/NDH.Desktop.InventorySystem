using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InventoryAppDataAccessLayer.Repositories.RepoImplementations;
using InventoryAppDataAccessLayer.Repositories.RepoInterfaces;
using InventoryAppServicesLayer.AuthorizationFilter;
using InventoryAppServicesLayer.ServiceImplementations;
using InventoryAppServicesLayer.ServiceInterfaces;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryManagementSystemUI
{
    public static class ServiceRegistration
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            services.AddScoped<IUserAuthenticationRepo, UserAuthenticationRepo>();
            services.AddScoped<IUserAuthenticationService, UserAuthenticationService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddSingleton<ILoggerConfigurator, LoggerConfigurator>();


            // Singleton if needed
            services.AddSingleton<JwtAuthorizationFilter>();

            return services;
        }
    }
}
