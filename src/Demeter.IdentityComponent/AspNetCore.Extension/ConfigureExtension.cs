using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Demeter.IdentityComponent;
using MongoDB.Driver;

namespace Demeter.IdentityComponent.AspNetCore.Extension
{
    public static class ConfigureExtension
    {
        public static void AddDemeterIdentity(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<IdentityOptions> optionsAction = null)
        {
            IConfigurationSection section = configuration.GetSection("DemeterIdentity");

            services.Configure<MongoDbSettings>(options => {
                options.ConnectionString = section.GetSection("ConnectionString").Value;
                options.Database = section.GetSection("Database").Value;
                options.UserCollection = section.GetSection("UserCollection").Value;
                options.RoleCollection = section.GetSection("RoleCollection").Value;
            });

            services.AddSingleton<IUserStore<DemeterUserIdentity>>(provider => {
                var options = provider.GetService<IOptions<MongoDbSettings>>();
                var client = new MongoClient(options.Value.ConnectionString);
                var database = client.GetDatabase(options.Value.Database);

                return new DemeterUserStore<DemeterUserIdentity>(
                    database,
                    options.Value.UserCollection);
            });

            services.AddSingleton<IRoleStore<DemeterRole>>(provider => {
                var options = provider.GetService<IOptions<MongoDbSettings>>();
                var client = new MongoClient(options.Value.ConnectionString);
                var database = client.GetDatabase(options.Value.Database);

                return new DemeterRoleStore<DemeterRole>(
                    database,
                    options.Value.RoleCollection);
            });

            services.AddIdentity<DemeterUserIdentity, DemeterRole>(optionsAction)
                .AddDefaultTokenProviders();
        }
    }
}