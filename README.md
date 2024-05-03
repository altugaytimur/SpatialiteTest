Spatiality:
- Spatialite is the addition of spatial data storage and querying capabilities of the Sqlite file-based database engine.
- In spatialite files, you can store verbal data as well as geometric data (point, line, area) in relational tables and use this data to create various data.
Geographic queries (intersection, closest distance, Coordinate, etc.) can be made.

template.sqlite:
  - There are 3 tables in the file named building, building_nodes, door.
     
How the program works:
  - The program is a console application
  - When the user runs the program, 3 options appear,
  - The user can run these 3 options in the order he wants by entering commands from the keyboard,
  - In option 1, by reading the records in the building_nodes table in the template database, building geometric objects are created according to the building_id and node_order columns and recorded in the building table. If the relevant building already exists in the table, it is deleted and added again.
  - In the 2nd option, by reading the door table, the location of each door is compared with the building table according to building_id, and whether the relevant door is inside or outside the building is determined by using the spatialite library spatial analysis functions, and the results are added to the inside_building column in the door table, "1" for inside. " is updated to "0" for outside.
    If there is no record in the building table for the relevant door, the inside_building column is updated to null.
  - In the 3rd option, the door results are printed on the screen as the example below;
* Door id: 1, Building id: 2, Door No: 2A, Is it inside a building: Yes -- record with inside_building column 1
* Door id: 2, Building id: 3, Door No: 16, Inside the building: No Record with --inside_building column 0
* Door id: 3, Building id: 1, Door No: 12, Inside the Building: Could Not Detect --Inside_building column's null record
