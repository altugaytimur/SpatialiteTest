using Business.Contracts;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Services;

public class DoorService : IDoorService
{
    private readonly SQLiteConnection _connection;

    public DoorService(SQLiteConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    /// <summary>
    /// Kapıların, binaların içinde mi dışında mı olduğunu kontrol eder ve içinde/dışında olduğuna göre inside_building   kolonunu günceller.
    /// </summary>

    public void CheckDoorsInsideBuildings()
    {
        string sqlSelectDoors = @"
            SELECT door.id, door.building_id, door.geom
            FROM door";

        using (var command = new SQLiteCommand(sqlSelectDoors, _connection))
        {
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int doorId = reader.GetInt32(0);
                    int? buildingId = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1);
                    var doorGeom = reader["geom"];

                    string sqlCheckInside = @"
                             UPDATE door SET inside_building =
                            CASE
                                WHEN @buildingId IS NULL THEN NULL -- Eşleşme yoksa null olacak
                                WHEN NOT EXISTS (
                                    SELECT 1 FROM building 
                                    WHERE building.id = @buildingId
                                ) THEN NULL -- Eşleşme yoksa null olacak
                                WHEN EXISTS (
                                    SELECT 1 FROM building 
                                    WHERE ST_Contains(building.geom, @doorGeom)
                                    AND building.id = @buildingId
                                ) THEN 1 -- Eşleşme varsa ve kapı bina içindeyse 1 olacak
                                ELSE 0 -- Eşleşme varsa ve kapı bina dışında ise 0 olacak
                            END 
                        WHERE id = @doorId";


                    using (var updateCommand=new SQLiteCommand(sqlCheckInside, _connection))
                    {
                        updateCommand.Parameters.AddWithValue("@doorGeom", doorGeom);
                        updateCommand.Parameters.AddWithValue("@buildingId", buildingId ?? (object)DBNull.Value);
                        updateCommand.Parameters.AddWithValue("@doorId", doorId);

                        int affectedRows = updateCommand.ExecuteNonQuery();

                        Console.WriteLine($"Kapı ID {doorId}: Konumu belirlendi, {affectedRows} kayıt güncellendi.");
                    }

                }
            }
        }
    }


    /// <summary>
    /// Kapıların sonuçlarını konsola yazdırır.
    /// </summary>
    public void PrintDoorResults()
    {
        string sqlSelect = "SELECT id, building_id, door_no, inside_building FROM door";

        using (var command = new SQLiteCommand(sqlSelect, _connection))
        {
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string insideBuilding = reader["inside_building"] is DBNull ? "Tespit Edilemedi" : (reader.GetInt32(3) == 1 ? "Evet" : "Hayır");

                    Console.WriteLine($"Kapı id: {reader.GetInt32(0)}, Bina id: {reader.GetInt32(1)}, Kapı No: {reader.GetString(2)}, Bina İçinde Mi: {insideBuilding}");

                }
            }
        }
    }

    /// <summary>
    /// Kapı tablosunun şemasını konsola yazdırır.
    /// </summary>
    void PrintDoorSchema()
    {
        string sqlPragma = "PRAGMA table_info(door)";

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

    /// <summary>
    /// Building_nodes tablosunun şemasını konsola yazdırır.
    /// </summary>
    void PrintBuildingNodesSchema()
    {
        string sqlPragma = "PRAGMA table_info(building_nodes)";

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
