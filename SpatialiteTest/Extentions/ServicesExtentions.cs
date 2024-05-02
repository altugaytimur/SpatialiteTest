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
            var dbService = new DbService(GetConnectionString());

            services.AddSingleton(dbService);
            services
                .AddScoped<SQLiteConnection>(provider =>
                        provider.GetService<DbService>().CreateConnection());

            services.AddScoped<IBuiildingService,BuildingService>();
            services.AddScoped<IDoorService,DoorService>();
            services.AddSingleton<App>();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static string GetConnectionString()
        {
            string pathTemplate = @"C:\Users\HP\Desktop\SpatialiteTest\SpatialiteTest\SpatialiteTest\template.sqlite";
            string destDbFilename = @"C:\Users\HP\Desktop\SpatialiteTest\SpatialiteTest\SpatialiteTest\buildingDatabaseNew.sqlite";
            if (File.Exists(destDbFilename))
            {
                File.Delete(destDbFilename);
            }
            File.Copy(pathTemplate, destDbFilename, true);

            string connectionString = $"Data Source={destDbFilename};Version=3;";
            return connectionString;
        }
    }
}
