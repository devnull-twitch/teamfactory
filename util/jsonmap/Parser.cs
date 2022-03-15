using Godot;
using Godot.Collections;
using TeamFactory.Map;
using TeamFactory.Items;

namespace TeamFactory.Util.JsonMap
{
    public class Parser 
    {
        private JSONParseResult mapRootJson;

        public Parser(string jsonStr)
        {
            mapRootJson = JSON.Parse(jsonStr);
        }

        public MapResource CreateMapData()
        {
            Dictionary mapData = (Dictionary)mapRootJson.Result;

            MapResource mapResource = new MapResource();
            mapResource.Width = (int)((System.Single)mapData["width"]);
            mapResource.Height = (int)((System.Single)mapData["height"]);
            mapResource.Tiles = new Array<TileResource>();

            mapResource.Time = (int)((System.Single)mapData["time"]);

            Array unlockedItems = (Array)mapData["unlocked_items"];
            mapResource.UnlockedItems = new Array<string>();
            foreach(string itemName in unlockedItems)
            {
                mapResource.UnlockedItems.Add(itemName);
            }

            Dictionary spawnData = (Dictionary)mapData["spwan_position"];
            mapResource.SpawnPosition = new Vector2(
                (float)spawnData["x"],
                (float)spawnData["y"]
            );

            Array jsonInfras = (Array)mapData["infras"];
            foreach (Dictionary infraConfig in jsonInfras)
            {
                TileResource tileResource = new TileResource();

                int x = (int)((System.Single)infraConfig["x"]);
                int y = (int)((System.Single)infraConfig["y"]);
                tileResource.Coords = new Vector2(x, y);

                tileResource.InfraTypeIdentifier = (InfraType.TypeIdentifier)((System.Single)infraConfig["type_identifier"]);
                tileResource.IsFinal = (bool)infraConfig["is_final"];
                
                if (infraConfig.Contains("is_lock")) 
                    tileResource.IsLocked = (bool)infraConfig["is_lock"];
                
                tileResource.Direction = (GridManager.Direction)((System.Single)infraConfig["direction"]);
                
                if ((string)infraConfig["spawn_resource"] != "")
                {
                    tileResource.SpawnResource = GD.Load<ItemResource>((string)infraConfig["spawn_resource"]);
                }

                tileResource.Connections = new Dictionary<GridManager.Direction, ConnectionTarget>();
                Dictionary connectionsMap = (Dictionary)infraConfig["connections"];
                if (connectionsMap.Contains("1")) {
                    Dictionary connectionData = (Dictionary)connectionsMap["1"];
                    Vector2 targetCoords = parseTargetCoords(connectionData);
                    tileResource.Connections[GridManager.Direction.Up] = new ConnectionTarget(
                        targetCoords,
                        (GridManager.Direction)((System.Single)connectionData["target_direction"])
                    );
                }
                if (connectionsMap.Contains("2")) {
                    Dictionary connectionData = (Dictionary)connectionsMap["2"];
                    Vector2 targetCoords = parseTargetCoords(connectionData);
                    tileResource.Connections[GridManager.Direction.Down] = new ConnectionTarget(
                        targetCoords,
                        (GridManager.Direction)((System.Single)connectionData["target_direction"])
                    );
                }
                if (connectionsMap.Contains("3")) {
                    Dictionary connectionData = (Dictionary)connectionsMap["3"];
                    Vector2 targetCoords = parseTargetCoords(connectionData);
                    tileResource.Connections[GridManager.Direction.Left] = new ConnectionTarget(
                        targetCoords,
                        (GridManager.Direction)((System.Single)connectionData["target_direction"])
                    );
                }
                if (connectionsMap.Contains("4")) {
                    Dictionary connectionData = (Dictionary)connectionsMap["4"];
                    Vector2 targetCoords = parseTargetCoords(connectionData);
                    tileResource.Connections[GridManager.Direction.Right] = new ConnectionTarget(
                        targetCoords,
                        (GridManager.Direction)((System.Single)connectionData["target_direction"])
                    );
                }

                mapResource.Tiles.Add(tileResource);
            }

            return mapResource;
        }

        private Vector2 parseTargetCoords(Dictionary connectionData)
        {
            int targetX = (int)((System.Single)connectionData["target_coords_x"]);
            int targetY = (int)((System.Single)connectionData["target_coords_y"]);

            return new Vector2(targetX, targetY);
        }
    }
}