using Business.Contracts;
using Business.Services;
using DAL;
using Microsoft.Extensions.DependencyInjection;
using Presentation;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpatialiteTest.Extentions
{
    public static class ServicesExtentions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        public static void ConfigureServices(this IServiceCollection services )
        {
            var dbService = new DbService(ConnectionsExtensions.GetConnectionString());

            services.AddSingleton(dbService);
            services
                .AddScoped<SQLiteConnection>(provider =>
                        provider.GetService<DbService>().CreateConnection());

            services.AddScoped<IBuiildingService,BuildingService>();
            services.AddScoped<IDoorService,DoorService>();
            services.AddSingleton<App>();
        }
    }
}
