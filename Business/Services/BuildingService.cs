using Business.Contracts;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Services
{
    public class BuildingService : IBuiildingService
    {
        private readonly SQLiteConnection _connection;

        public BuildingService(SQLiteConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        /// <summary>
        /// Building_nodes tablosundaki düğümleri kullanarak yeni binalar oluşturur. Var olan binaların id'lerini kontrol eder, eğer varsa siler ve tekrar ekler.
        /// </summary>
        public void CreateBuildingsFromNodes()
        {
            
            string sqlCheck = "SELECT id FROM building WHERE id = @id";
            string sqlDelete = "DELETE FROM building WHERE id = @id";

            string sqlInsert = @"
                INSERT INTO building (id, geom)
                SELECT building_id, ST_MakePolygon(ST_AddPoint(line, StartPoint(line))) AS geom
                FROM (
                    SELECT building_id, MakeLine(geom) AS line, StartPoint(MakeLine(geom)) AS StartPoint
                    FROM building_nodes
                    GROUP BY building_id
                    ORDER BY building_id, node_order
                );";

            using (var commandCheck = new SQLiteCommand(sqlCheck, _connection))
            using (var commandDelete = new SQLiteCommand(sqlDelete, _connection))
            using (var commandInsert = new SQLiteCommand(sqlInsert, _connection))
            {
                
                string sqlSelectBuildingIds = "SELECT DISTINCT building_id FROM building_nodes";
                List<int> buildingIds = new List<int>();

                using (var commandSelectBuildingIds = new SQLiteCommand(sqlSelectBuildingIds, _connection))
                using (var reader = commandSelectBuildingIds.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        buildingIds.Add(reader.GetInt32(0));
                    }
                }

                foreach (int buildingId in buildingIds)
                {
                    commandCheck.Parameters.Clear();
                    commandCheck.Parameters.AddWithValue("@id", buildingId);
                    var existingBuildingId = commandCheck.ExecuteScalar();

                    if (existingBuildingId != null)
                    {
                        commandDelete.Parameters.Clear();
                        commandDelete.Parameters.AddWithValue("@id", buildingId);
                        commandDelete.ExecuteNonQuery();
                        Console.WriteLine($"Var olan bina (ID: {buildingId}) silindi ve tekrar eklenecek.");
                    }
                }
                commandInsert.ExecuteNonQuery();

                PrintBuilding();

            }
        }


        /// <summary>
        ///  Building tablosundaki bina verilerini konsola yazdırır.
        /// </summary>
        public void PrintBuilding()
        {
            string sqlQuery = "SELECT id, AsText(geom) as geom_text FROM building";

            using (var command = new SQLiteCommand(sqlQuery, _connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);

                        string geom = reader.IsDBNull(1) ? "Geometri yok" : reader.GetString(1);

                        Console.WriteLine($"ID: {id}, Geom: {geom}");
                    }
                }

            }
        }

        /// <summary>
        /// Building tablosunun şemasını konsola yazdırır.
        /// </summary>
        void PrintBuildingSchema()
        {
            string sqlPragma = "PRAGMA table_info(building)";

            using (var command = new SQLiteCommand(sqlPragma, _connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine($"Name: {reader["name"]}, Type: {reader["type"]}, PK: {reader["pk"]}");

                    }

                }

            }
        }
    }
}
