using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpatialiteTest.Extentions
{
    public static class ConnectionsExtensions
    {
        /// <summary>
        /// Bağlantı dizesini oluşturur.
        /// </summary>
        /// <returns> Oluşturulan bağlantı dizesi </returns>
        public static string GetConnectionString()
        {
            string destDbFilename = GetDatabaseFilename();
            string connectionString = $"Data Source={destDbFilename};Version=3;";
            return connectionString;
        }

        /// <summary>
        /// Veritabanı dosyasının konumunu belirler.
        /// </summary>
        /// <returns> Veritabanı dosyasının konumu </returns>
        private static string GetDatabaseFilename()
        {
            string pathTemplate = @"C:\Users\HP\Desktop\SpatialiteTest\SpatialiteTest\SpatialiteTest\template.sqlite";
            string destDbFilename = @"C:\Users\HP\Desktop\SpatialiteTest\SpatialiteTest\SpatialiteTest\buildingDatabaseNew.sqlite";
            if (File.Exists(destDbFilename))
            {
                File.Delete(destDbFilename);
            }
            File.Copy(pathTemplate, destDbFilename, true);

            return destDbFilename;
        }
    }
}
