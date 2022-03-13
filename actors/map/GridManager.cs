using Godot;
using TeamFactory.Conveyor;
using TeamFactory.Infra;
using TeamFactory.Factory;
using Godot.Collections;
using TeamFactory.Lib.JsonMap;
using TeamFactory.Lib.Multiplayer;

namespace TeamFactory.Map 
{
    public class GridManager
    {
        public enum Direction : int
        {
            Up = 1,
            Down = 2,
            Left = 3,
            Right = 4
        }

        const int CELLSIZE = 128;

        private int mapWidth;

        private int mapHeight;

        private AStar2D infraMap;

        private Node2D mapNode;

        private int offset;

        private Dictionary<int, string> infraCache = new Dictionary<int, string>();

        private Dictionary<int, Dictionary<Direction, Array<string>>> conveyorCache = new Dictionary<int, Dictionary<Direction, Array<string>>>();

        private Dictionary<int, int> playersReady = new Dictionary<int, int>();

        private Dictionary<string, int[]> connectionPathCache = new Dictionary<string, int[]>();

        public Parser Parser;

        public GridManager(Node2D mapNode, Parser parser)
        {
            this.mapNode = mapNode;
            this.Parser = parser;

            infraMap = new AStar2D();
        }

        public bool AddTileResouce(TileResource tr, int mapIndex)
        {
            if (infraMap.IsPointDisabled(mapIndex)) 
            {
                return false;
            }

            addInfraNode(tr, mapIndex);
            if (tr.Connections.Count > 0)
            {
                connectNode(tr, mapIndex);
            }
            return true;
        }

        public InfraSprite GetInfraAtIndex(int index)
        {
            if (!infraCache.ContainsKey(index))
            {
                return null;
            }
            
            return mapNode.GetNode<InfraSprite>(infraCache[index]);
        }

        public bool ConnectTileResource(int srcIndex, int target, Direction outputDir)
        {
            InfraSprite targetNode = GetInfraAtIndex(target); 
            if (targetNode == null)
                return false;

            InfraSprite sourceNode = GetInfraAtIndex(srcIndex);
            // check that output isnt already connected
            // TODO: maybe auto disconnect??
            if (sourceNode.OutConnections.ContainsKey(outputDir))
                return false;   

            int startIndex = GetIndicesFromDirection(srcIndex, outputDir);
            Direction bestInDir = Direction.Left;
            float currentBestPathCost = 0;
            foreach (Direction inDir in targetNode.Type.Inputs)
            {
                if (targetNode.InConnections.ContainsKey(inDir))
                    continue;
                
                int endIndex = GetIndicesFromDirection(target, inDir);
                int[] path = infraMap.GetIdPath(startIndex, endIndex);
                float cost = 0;
                foreach(int pathPointID in path)
                {
                    cost += infraMap.GetPointWeightScale(pathPointID);
                }
                if (currentBestPathCost == 0 || cost < currentBestPathCost)
                {
                    bestInDir = inDir;
                    currentBestPathCost = cost;
                }
            }

            if (currentBestPathCost == 0)
            {
                GD.Print("no available input found on target");
                return false;
            }

            Vector2 targetMapCoords = IndexToMap(target);
            ConnectionTarget conTarget = new ConnectionTarget(targetMapCoords, bestInDir);
            
            sourceNode.OutConnections[outputDir] = conTarget;
            connectConnection(srcIndex, outputDir, conTarget);
            NetState.Rpc(sourceNode, "UpdateOutConnection", outputDir, (int)targetMapCoords.x, (int)targetMapCoords.y, bestInDir);

            return true;
        }

        public void DisconnectTileResource(int srcIndex, Direction outDirection)
        {
            if (!conveyorCache.ContainsKey(srcIndex))
            {
                throw new System.Exception("No connection found");
            }
            if (!conveyorCache[srcIndex].ContainsKey(outDirection))
            {
                throw new System.Exception("No connection found for output direction");
            }

            foreach (string nodeName in conveyorCache[srcIndex][outDirection])
            {
                ConveyorNode conveyorNode = mapNode.GetNode<ConveyorNode>(nodeName);
                NetState.Rpc(conveyorNode, "TriggereDeleteion");
                infraMap.SetPointWeightScale(WorldToIndex(conveyorNode.GlobalPosition), 1f);
            }
        }

        public void Cleanup()
        {
            infraMap = new AStar2D();
            infraCache = new Dictionary<int, string>();
            offset = 0;

            foreach(Node n in mapNode.GetChildren())
            {
                n.QueueFree();
            }

            TileMap floor = mapNode.GetNode<TileMap>("../Floor");
            floor.Clear();
        }

        public void ClientInit()
        {
            MapResource map = Parser.CreateMapData();
            mapWidth = map.Width;
            mapHeight = map.Height;
        }

        public void AddPlayerZone(int ownerNetID, string color = "#EEEEEE")
        {
            MapResource map = Parser.CreateMapData();
            mapWidth = map.Width;
            mapHeight = map.Height;

            NetState.Rpc(mapNode, "SetupPlayerFloor", color, offset);

            // moving player to factory
            int relSpawnPosIndex = MapToIndex(map.SpawnPosition);
            int absoluteSpawnPosIndex = offset + relSpawnPosIndex;
            Vector2 spawnPos = IndexToWorld(absoluteSpawnPosIndex);

            // place player on spawn position
            NetState.Rpc(mapNode, "RelocatePlayer", ownerNetID, spawnPos.x, spawnPos.y);

            int relMaxIndex = mapWidth * mapHeight;
            for (int i = 0; i < relMaxIndex; i++)
            {
                int absoluteIndex = offset + i;
                Vector2 relCoords = IndexToMap(i);
                Vector2 absCoords = IndexToMap(absoluteIndex);
                infraMap.AddPoint(absoluteIndex, absCoords, 1);
                if (relCoords.x > 0) {
                    infraMap.ConnectPoints(absoluteIndex, absoluteIndex - 1, true);
                }
                if (relCoords.y > 0)
                {
                    infraMap.ConnectPoints(absoluteIndex, absoluteIndex - map.Width, true);
                }
            }

            for (int i = 0; i < map.Tiles.Count; i++)
            {
                TileResource tr = map.Tiles[i];
                tr.OwnerID = ownerNetID;
                
                Vector2 relativeMapPos = tr.Coords;
                int relativeIndex = MapToIndex(relativeMapPos);
                int absoluteIndex = offset + relativeIndex;

                addInfraNode(tr, absoluteIndex);
            }

            for (int i = 0; i < map.Tiles.Count; i++)
            {
                TileResource tr = map.Tiles[i];
                Vector2 relativeMapPos = tr.Coords;
                int relativeIndex = MapToIndex(relativeMapPos);
                int absoluteIndex = offset + relativeIndex;

                Dictionary<Direction, ConnectionTarget> fixedConnectionDict = new Dictionary<Direction, ConnectionTarget>();
                foreach(System.Collections.Generic.KeyValuePair<Direction, ConnectionTarget> tuple in tr.Connections)
                {
                    int relIndex = MapToIndex(tuple.Value.TargetCoords);
                    Vector2 fixedCoords = IndexToMap(relIndex + offset);

                    fixedConnectionDict[tuple.Key] = new ConnectionTarget(fixedCoords, tuple.Value.Direction);
                }
                tr.Connections = fixedConnectionDict;
                
                if (tr != null && tr.Connections.Count > 0)
                {
                    connectNode(tr, absoluteIndex);
                }
            }

            playersReady[ownerNetID] = 0;
            NetState.Rpc(mapNode, "AckReady", ownerNetID);

            offset += mapWidth * (mapHeight + 1);
        }

        public void IncAckPlayerReady(int playerID)
        {
            playersReady[playerID] += 1;

            foreach (int readyCounts in playersReady.Values)
            {
                if (readyCounts != playersReady.Count)
                    return;
            }

            foreach (int playerKey in playersReady.Keys)
            {
                playersReady[playerKey] = 0;
            }

            Physics2DServer.SetActive(true);
            GD.Print("Physics2DServer active!");
        }

        public void AddPlayerFloor(string color, int playerOffset)
        {
            TileMap floor = mapNode.GetNode<TileMap>("../Floor");
            int newTileID = floor.TileSet.GetLastUnusedTileId();
            Texture floorTexture = GD.Load<Texture>("res://actors/floor/BaseGround.png");
            ShaderMaterial shaderMaterial = GD.Load<ShaderMaterial>("res://materials/FloorA.tres");
            shaderMaterial = (ShaderMaterial)shaderMaterial.Duplicate();
            shaderMaterial.SetShaderParam("TeamColor", new Color(color));

            floor.TileSet.CreateTile(newTileID);
            floor.TileSet.TileSetTexture(newTileID, floorTexture);
            floor.TileSet.TileSetMaterial(newTileID, shaderMaterial);

            Vector2 offsetCoords = IndexToMap(playerOffset);

            for (int x = 0; x < mapWidth; x++)
            {
                floor.SetCell((int)offsetCoords.x + x, (int)offsetCoords.y - 1, 1, false, false, true);
                for (int y = 0; y < mapHeight; y++)
                {
                    if (x == 0)
                    {
                        floor.SetCell((int)offsetCoords.x - 1, (int)offsetCoords.y + y, 1);
                    }
                    floor.SetCell((int)offsetCoords.x + x, (int)offsetCoords.y + y, newTileID);
                    if (x == mapWidth - 1)
                    {
                        floor.SetCell((int)offsetCoords.x + mapWidth, (int)offsetCoords.y + y, 1);
                    }
                }
                floor.SetCell((int)offsetCoords.x + x, (int)offsetCoords.y + mapHeight, 1, false, false, true);
            }
        }

        public int[] GetPathTo(int from, int to)
        {
            return infraMap.GetIdPath(from, to);
        }

        public Vector2 IndexToMap(int i)
        {
            int x = i % mapWidth;
            int y = i / mapWidth;

            return new Vector2(x, y);
        }

        public int MapToIndex(Vector2 v)
        {
            int index = (int)v.y * mapWidth + (int)v.x;

            return index;
        }

        public Vector2 IndexToWorld(int i)
        {
            float x = i % mapWidth * CELLSIZE + (CELLSIZE / 2);
            float y = i / mapWidth * CELLSIZE + (CELLSIZE / 2);

            return new Vector2(x, y);
        }

        public int WorldToIndex(Vector2 position)
        {
            int x = (int)position.x / CELLSIZE;
            int y = (int)position.y / CELLSIZE;

            if (x < 0 || y < 0)
            {
                throw new OutOfMapException();
            }

            return MapToIndex(new Vector2(x, y));
        }

        public Vector2[] IndicesToWorld(int[] indices)
        {
            Vector2[] res = new Vector2[indices.Length];
            for (int i = 0; i < indices.Length; i++)
            {
                res[i] = IndexToWorld(indices[i]);
            }

            return res;
        }

        private void addInfraNode(TileResource tr, int index)
        {
            string infraNodeName = $"InfraNode_{index}";
            string spawnResourceName  = "";
            if (tr.SpawnResource != null)
            {
                spawnResourceName = tr.SpawnResource.Name;
            }
            
            NetState.Rpc(
                mapNode,
                "CreateInfraNode",
                infraNodeName,
                tr.InfraTypeIdentifier,
                index,
                tr.Direction,
                spawnResourceName,
                tr.OwnerID
            );
            infraMap.SetPointDisabled(index, true);

            InfraType infraType = InfraType.GetByIdentifier(tr.InfraTypeIdentifier);

            foreach(Direction inputDir in infraType.Inputs)
            {
                int inputIndex = GetIndicesFromDirection(index, inputDir);
                infraMap.SetPointWeightScale(inputIndex, 3f);
            }

            foreach(Direction outputDir in infraType.Outputs)
            {
                int inputIndex = GetIndicesFromDirection(index, outputDir);
                infraMap.SetPointWeightScale(inputIndex, 3f);
            }

            infraCache[index] = infraNodeName;
        }

        private void connectNode(TileResource tr, int index)
        {
            foreach(System.Collections.Generic.KeyValuePair<Direction, ConnectionTarget> tuple in tr.Connections)
            {
                connectConnection(index, tuple.Key, tuple.Value);
            }
        }

        private void connectConnection(int srcIndex, Direction outDirection, ConnectionTarget target)
        {
            int targetAbsIndex = MapToIndex(target.TargetCoords);
            InfraSprite srcNode = GetInfraAtIndex(srcIndex);
            int startIndex = GetIndicesFromDirection(srcIndex, outDirection);
            int endIndex = GetIndicesFromDirection(targetAbsIndex, target.Direction);

            NetState.Rpc(mapNode, "SaveConnection", srcIndex, outDirection, targetAbsIndex, target.Direction);

            // make path without source and dest
            int[] path = infraMap.GetIdPath(startIndex, endIndex);
            int[] completePath = new int[path.Length + 2];
            System.Array.Copy(path, 0, completePath, 1, path.Length);
            completePath[0] = srcIndex;
            // add in source and dest ( blocked in a star because they are infra )
            completePath[completePath.Length - 1] = targetAbsIndex;

            string pathCacheKey = $"{srcIndex}_{targetAbsIndex}";
            connectionPathCache[pathCacheKey] = completePath;

            if (!conveyorCache.ContainsKey(srcIndex))
            {
                conveyorCache[srcIndex] = new Dictionary<Direction, Array<string>>();
            }

            Array<string> connectionConveyorList = new Array<string>();
            for(int j = 1; j < completePath.Length - 1; j++)
            {
                string conveyorNodeName = $"Conveyor_{srcIndex}_{completePath[j]}";
                Direction inputDir = GetDirectionFromIndices(completePath[j], completePath[j-1]);
                Direction outputDir = GetDirectionFromIndices(completePath[j], completePath[j+1]);
                NetState.Rpc(mapNode, "CreateConveyorNode", conveyorNodeName, completePath[j], inputDir, outputDir);

                infraMap.SetPointWeightScale(completePath[j], 3f);
                connectionConveyorList.Add(conveyorNodeName);
            }

            conveyorCache[srcIndex][outDirection] = connectionConveyorList;
        }

        public int[] GetConnectionPath(int srcIndex, int targetIndex)
        {
            string pathCacheKey = $"{srcIndex}_{targetIndex}";
            if (!connectionPathCache.ContainsKey(pathCacheKey))
                return new int[0];

            return connectionPathCache[pathCacheKey];
        }

        public static Direction GetDirectionFromIndices(int selfIndex, int targetIndex)
        {
            int diff = targetIndex - selfIndex;
            switch (diff)
            {
                case 1:
                    return Direction.Right;
                case -1:
                    return Direction.Left;
                case 15:
                    return Direction.Down;
                case -15:
                    return Direction.Up;
            }

            throw new System.Exception($"target {targetIndex} is not next to self {selfIndex}");
        }

        public static int GetIndicesFromDirection(int selfIndex, Direction dir)
        {
            switch (dir)
            {
                case Direction.Right:
                    return selfIndex + 1;
                case Direction.Left:
                    return selfIndex - 1;
                case Direction.Down:
                    return selfIndex + 15;
                case Direction.Up:
                    return selfIndex - 15;
            }

            throw new System.Exception($"Invalid direction {dir}");
        }
    }
}