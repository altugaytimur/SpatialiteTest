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
        /// 
        /// </summary>
        public void CreateBuildingsFromNodes()
        {
            string sqlDelete = "DELETE FROM building";
            using (var command=new SQLiteCommand(sqlDelete,_connection))
            {
                command.ExecuteNonQuery();
            }

            string sqlInsert = @"
            INSERT INTO building(id,geom)
            SELECT building_id, ST_MakePolygon(ST_AddPoint(line, StartPoint(line))) AS geom 
            FROM(
                SELECT building_id,MakeLine(geom) AS line, StartPoint(MakeLine(geom)) AS StartPoint
                FROM building_nodes
                GROUP BY building_id
                ORDER BY building_id, node_order
            );";

            using(var command=new SQLiteCommand(sqlInsert, _connection))
            {
                command.ExecuteNonQuery();
            }
            
        }


        /// <summary>
        /// 
        /// </summary>
        public void PrintBuilding()
        {
            string sqlQuery = "SELECT id, AsText(geom) as geom_text FROM building";

            using(var command = new SQLiteCommand(sqlQuery, _connection))
            {
                using(var reader=command.ExecuteReader())
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
        /// 
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
