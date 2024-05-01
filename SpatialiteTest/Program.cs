using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.Data.SQLite;


string pathTemplate = @"C:\Users\HP\Desktop\SpatialiteTest\SpatialiteTest\SpatialiteTest\template.sqlite";
string destDbFileName= @"C:\Users\HP\Desktop\SpatialiteTest\SpatialiteTest\SpatialiteTest\buildingDatabase.sqlite";

if (File.Exists(destDbFileName))
{
    File.Delete(destDbFileName);
}
File.Copy(pathTemplate, destDbFileName, true);

string connectionString = $"Data Source={destDbFileName};Version=3;";

string modSpatialitePath = @"C:\Users\HP\Desktop\SpatialiteTest\SpatialiteTest\SpatialiteTest\mod_spatialite-5.1.0-win-amd64";
string currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
Environment.SetEnvironmentVariable("PATH", currentPath + ";" + modSpatialitePath, EnvironmentVariableTarget.Process);


using (var connection = new SQLiteConnection(connectionString))
{
    try
    {
        connection.Open();
        connection.EnableExtensions(true);
        connection.LoadExtension("mod_spatialite.dll");

        PrintBuildingSchema(connection);

        while (true)
        {
            Console.WriteLine("1. Binaları Oluştur\n2. Kapı Kontrolü\n3. Kapı Sonuçlarını Yazdır\n4. Çıkış Yap\nLütfen bir seçenek giriniz:");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    CreateBuildingsFromNodes(connection);
                    break;
                case "2":
                    CheckDoorsInsideBuildings(connection);
                    break;
                case "3":
                    PrintDoorResults(connection);
                    break;
                case "4":
                    Console.WriteLine("Programdan çıkılıyor...");
                    return;
                default:
                    Console.WriteLine("Geçersiz seçim. Lütfen tekrar deneyin.");
                    break;
            }
        }

    }
    catch (Exception ex)
    {
        Console.WriteLine("Hata: " + ex.Message);
    }
    finally
    {
        connection.Close();

    }
}

    static void CreateBuildingsFromNodes(SQLiteConnection connection)
{
    var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    string sqlDelete = "DELETE FROM building";
    using (var command = new SQLiteCommand(sqlDelete, connection))
    {
        command.ExecuteNonQuery();
    }

    string sqlSelect = @"
        SELECT building_id, AsText(geom) AS geom
        FROM building_nodes
        ORDER BY building_id, node_order";

    List<Coordinate> coordinates = new List<Coordinate>();
    int currentBuildingId = -1;
    using (var command = new SQLiteCommand(sqlSelect, connection))
    {
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                int buildingId = reader.GetInt32(0);
                var geom = reader.GetString(1);
                var coordinate = new WKTReader().Read(geom).Coordinate;

                if (buildingId != currentBuildingId && currentBuildingId != -1)
                {
                    if (coordinates.Count > 0 && !coordinates[0].Equals2D(coordinates[coordinates.Count - 1]))
                    {
                        coordinates.Add(coordinates[0]);
                    }
                    SaveBuilding(connection, currentBuildingId, coordinates, geometryFactory);
                    coordinates.Clear();
                }

                coordinates.Add(coordinate);
                currentBuildingId = buildingId;

            }


            if (coordinates.Count > 0)
            {
                if (!coordinates[0].Equals2D(coordinates[coordinates.Count - 1]))
                {
                    coordinates.Add(coordinates[0]);
                }

                SaveBuilding(connection, currentBuildingId, coordinates, geometryFactory);
            }
        }
    }

    PrintBuilding(connection);

}



static void SaveBuilding(SQLiteConnection connection, int buildingId, List<Coordinate> coordinates, GeometryFactory geometryFactory)
{
    var linearRing = geometryFactory.CreateLinearRing(coordinates.ToArray());
    var polygon = geometryFactory.CreatePolygon(linearRing);

    string sqlInsert = "INSERT INTO building (id, geom) VALUES (@id, GeomFromText(@geom, 4326))";

    using (var cmd = new SQLiteCommand(sqlInsert, connection))
    {
        cmd.Parameters.AddWithValue("@id", buildingId);
        cmd.Parameters.AddWithValue("@geom", new WKTWriter().Write(polygon));
        cmd.ExecuteNonQuery();

    }

}

static void CheckDoorsInsideBuildings(SQLiteConnection connection)
{

    string sqlSelectDoors = @"
            SELECT door.id, door.building_id, door.geom
            FROM door";

    using (var command = new SQLiteCommand(sqlSelectDoors, connection))
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
                                WHEN @buildingId IS NULL THEN NULL
                                WHEN EXISTS (
                                    SELECT 1 FROM building 
                                    WHERE ST_Contains(building.geom, @doorGeom)
                                    AND building.id = @buildingId
                                ) THEN 1
                                ELSE 0
                            END 
                        WHERE id = @doorId";

                using (var updateCommand = new SQLiteCommand(sqlCheckInside, connection))
                {
                    updateCommand.Parameters.AddWithValue("@doorGeom", doorGeom);
                    updateCommand.Parameters.AddWithValue("@buildingId", buildingId ?? (object)DBNull.Value);
                    updateCommand.Parameters.AddWithValue("@doorId", doorId);

                    int affectedRows = updateCommand.ExecuteNonQuery();

                    Console.WriteLine($"Kapı ID {doorId}: Konumu güncellendi, {affectedRows} kayıt etkilendi.");

                }
            }
        }
    }
}



static void PrintDoorResults(SQLiteConnection connection)
{
    string sqlSelect = "SELECT id, building_id, door_no, inside_building FROM door";

    using (var command = new SQLiteCommand(sqlSelect, connection))
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


static void PrintBuilding(SQLiteConnection connection)
{
    string sqlQuery = "SELECT id, AsText(geom) as geom_text FROM building";

    using (var command = new SQLiteCommand(sqlQuery, connection))
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


static void PrintBuildingSchema(SQLiteConnection connection)
{
    string sqlPragma = "PRAGMA table_info(building_nodes)";

    using (var command = new SQLiteCommand(sqlPragma, connection))
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