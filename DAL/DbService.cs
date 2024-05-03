using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class DbService
    {
        private readonly string _connectionString;
        public DbService(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// SQLite bağlantısı oluşturur ve mod_spatialite eklentisini yükler.
        /// </summary>
        /// <returns> Oluşturulan SQLite bağlantısı </returns>
        public SQLiteConnection CreateConnection()
        {
            string modSpatialitePath = @"C:\Users\HP\Desktop\SpatialiteTest\SpatialiteTest\mod_spatialite-5.1.0-win-amd64";
            string currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PATH", currentPath+";"+modSpatialitePath,EnvironmentVariableTarget.Process);

            var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            connection.EnableExtensions(true);
            connection.LoadExtension("mod_spatialite.dll");
            return connection;
        }
    }
}
