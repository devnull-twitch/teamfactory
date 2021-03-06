using Godot;
using TeamFactory.Conveyor;
using TeamFactory.Infra;
using Godot.Collections;
using TeamFactory.Util.JsonMap;
using TeamFactory.Util.Multiplayer;

namespace TeamFactory.Map 
{
    public class GridManager
    {
        public enum Direction : int
        {
            Right = 1,
            Down = 2,
            Left = 3,
            Up = 4
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

        public void RemoveTileResource(int mapIndex)
        {
            infraMap.SetPointDisabled(mapIndex, false);
            InfraSprite infra = GetInfraAtIndex(mapIndex);
            
            foreach (Direction outDir in infra.OutConnections.Keys)
                DisconnectTileResource(mapIndex, outDir);

            foreach (ConnectionTarget target in infra.InConnections.Values)
                DisconnectTileResource(MapToIndex(target.TargetCoords), target.Direction);

            infraCache.Remove(mapIndex);
            NetState.Rpc(infra, "TriggereDeletion");
        }

        public void AddBlockingResource(int index)
        {
            addBlockingNode(index);
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

            int startIndex = GetIndicesFromDirection(mapWidth, srcIndex, outputDir);
            Direction bestInDir = Direction.Left;
            float currentBestPathCost = 0;
            foreach (Direction inDir in targetNode.Type.GetInputs(targetNode.Direction))
            {
                GD.Print($"testing a connection to {inDir} and target base direction is {targetNode.Direction}");
                if (targetNode.InConnections.ContainsKey(inDir))
                    continue;
                
                int endIndex = GetIndicesFromDirection(mapWidth, target, inDir);
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

            try
            {
                connectConnection(srcIndex, outputDir, conTarget);
            }
            catch (PathDuplicationException)
            {
                return false;
            }

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

            InfraSprite srcNode = mapNode.GetNode<InfraSprite>(infraCache[srcIndex]);
            // clear input direction so we can reconnect to that
            int targetIndex = MapToIndex(srcNode.OutConnections[outDirection].TargetCoords);
            InfraSprite targetNode = mapNode.GetNode<InfraSprite>(infraCache[targetIndex]);
            targetNode.InConnections.Remove(srcNode.OutConnections[outDirection].Direction);
            // remove out connection on source node
            srcNode.OutConnections.Remove(outDirection);

            string pathCacheKey = $"{srcIndex}_{targetIndex}";
            connectionPathCache.Remove(pathCacheKey);
            
            // inform client about cleared output state ( client does not care about input state; that is server only )
            NetState.Rpc(srcNode, "ClearOutConnection", outDirection);

            foreach (string nodeName in conveyorCache[srcIndex][outDirection])
            {
                ConveyorNode conveyorNode = mapNode.GetNode<ConveyorNode>(nodeName);
                NetState.Rpc(conveyorNode, "TriggereDeleteion");
                infraMap.SetPointWeightScale(WorldToIndex(conveyorNode.GlobalPosition), 1f);
            }
        }

        public void Cleanup()
        {
            GD.Print("clear old map data");
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

            for (int i = 0; i < map.Blockings.Count; i++)
            {
                BlockingResource br = map.Blockings[i];
                
                Vector2 relativeMapPos = br.Coords;
                int relativeIndex = MapToIndex(relativeMapPos);
                int absoluteIndex = offset + relativeIndex;

                addBlockingNode(absoluteIndex);
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

            // top left corner
            floor.SetCell((int)offsetCoords.x - 1, (int)offsetCoords.y - 1, 4, false, false, true);
            int aboveTileIndex = floor.GetCell((int)offsetCoords.x - 1, (int)offsetCoords.y - 2);
            if (aboveTileIndex != Godot.TileMap.InvalidCell)
            {
                floor.SetCell((int)offsetCoords.x - 1, (int)offsetCoords.y - 1, 3, false, false, false);
                int leftTileIndex = floor.GetCell((int)offsetCoords.x - 2, (int)offsetCoords.y - 1);
                if (leftTileIndex != Godot.TileMap.InvalidCell)
                    floor.SetCell((int)offsetCoords.x - 1, (int)offsetCoords.y - 1, 2);
            }

            // top right corner
            floor.SetCell((int)offsetCoords.x + mapWidth, (int)offsetCoords.y - 1, 4, true, false, false);
            aboveTileIndex = floor.GetCell((int)offsetCoords.x + mapWidth, (int)offsetCoords.y - 2);
            if (aboveTileIndex != Godot.TileMap.InvalidCell)
            {
                floor.SetCell((int)offsetCoords.x + mapWidth, (int)offsetCoords.y - 1, 3, true, false, false);
                int leftTileIndex = floor.GetCell((int)offsetCoords.x + mapWidth + 1, (int)offsetCoords.y - 1);
                if (leftTileIndex != Godot.TileMap.InvalidCell)
                    floor.SetCell((int)offsetCoords.x + mapWidth, (int)offsetCoords.y - 1, 2);
            }

            // bottom left corner
            floor.SetCell((int)offsetCoords.x - 1, (int)offsetCoords.y + mapHeight, 4, false, true, true);
            aboveTileIndex = floor.GetCell((int)offsetCoords.x - 1, (int)offsetCoords.y + mapHeight + 1);
            if (aboveTileIndex != Godot.TileMap.InvalidCell)
            {
                floor.SetCell((int)offsetCoords.x - 1, (int)offsetCoords.y + mapHeight, 3, false, true, false);
                int leftTileIndex = floor.GetCell((int)offsetCoords.x - 2, (int)offsetCoords.y + mapHeight);
                if (leftTileIndex != Godot.TileMap.InvalidCell)
                    floor.SetCell((int)offsetCoords.x - 1, (int)offsetCoords.y + mapHeight, 2);
            }

            // bottom right corner
            floor.SetCell((int)offsetCoords.x + mapHeight, (int)offsetCoords.y + mapHeight, 4, true, true, true);
            aboveTileIndex = floor.GetCell((int)offsetCoords.x + mapWidth, (int)offsetCoords.y + mapHeight + 1);
            if (aboveTileIndex != Godot.TileMap.InvalidCell)
            {
                floor.SetCell((int)offsetCoords.x + mapHeight, (int)offsetCoords.y + mapHeight, 3, true, false, false);
                int leftTileIndex = floor.GetCell((int)offsetCoords.x + mapWidth + 1, (int)offsetCoords.y + mapHeight);
                if (leftTileIndex != Godot.TileMap.InvalidCell)
                    floor.SetCell((int)offsetCoords.x + mapHeight, (int)offsetCoords.y + mapHeight, 2);
            }
            
            for (int x = 0; x < mapWidth; x++)
            {
                // top wall
                floor.SetCell((int)offsetCoords.x + x, (int)offsetCoords.y - 1, 1, false, false, true);
                for (int y = 0; y < mapHeight; y++)
                {
                    // left wall
                    if (x == 0)
                    {
                        floor.SetCell((int)offsetCoords.x - 1, (int)offsetCoords.y + y, 1);
                    }

                    // normal floor
                    floor.SetCell((int)offsetCoords.x + x, (int)offsetCoords.y + y, newTileID);
                    
                    // rigth wall 
                    if (x == mapWidth - 1)
                    {
                        floor.SetCell((int)offsetCoords.x + mapWidth, (int)offsetCoords.y + y, 1);
                    }
                }

                // bottom wall
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

        private void addBlockingNode(int index)
        {
            string infraNodeName = $"InfraNode_{index}";
            
            NetState.Rpc(
                mapNode,
                "CreateBlockingNode",
                infraNodeName,
                index
            );
            infraMap.SetPointDisabled(index, true);
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
                int inputIndex = GetIndicesFromDirection(mapWidth, index, inputDir);
                infraMap.SetPointWeightScale(inputIndex, 3f);
            }

            foreach(Direction outputDir in infraType.Outputs)
            {
                int inputIndex = GetIndicesFromDirection(mapWidth, index, outputDir);
                infraMap.SetPointWeightScale(inputIndex, 3f);
            }

            infraCache[index] = infraNodeName;
        }

        private void connectNode(TileResource tr, int index)
        {
            foreach(System.Collections.Generic.KeyValuePair<Direction, ConnectionTarget> tuple in tr.Connections)
            {
                try 
                {
                    connectConnection(index, tuple.Key, tuple.Value);
                }
                catch (PathDuplicationException)
                {
                    GD.Print("Path setup error: duplicated path target");
                }
            }
        }

        private void connectConnection(int srcIndex, Direction outDirection, ConnectionTarget target)
        {
            int targetAbsIndex = MapToIndex(target.TargetCoords);
            InfraSprite srcNode = GetInfraAtIndex(srcIndex);
            int startIndex = GetIndicesFromDirection(mapWidth, srcIndex, outDirection);
            int endIndex = GetIndicesFromDirection(mapWidth, targetAbsIndex, target.Direction);

            string pathCacheKey = $"{srcIndex}_{targetAbsIndex}";
            if (connectionPathCache.ContainsKey(pathCacheKey))
                throw new PathDuplicationException();

            NetState.Rpc(mapNode, "SaveConnection", srcIndex, outDirection, targetAbsIndex, target.Direction);

            // make path without source and dest
            int[] path = infraMap.GetIdPath(startIndex, endIndex);
            int[] completePath = new int[path.Length + 2];
            System.Array.Copy(path, 0, completePath, 1, path.Length);
            completePath[0] = srcIndex;
            // add in source and dest ( blocked in a star because they are infra )
            completePath[completePath.Length - 1] = targetAbsIndex;
            connectionPathCache[pathCacheKey] = completePath;
            
            if (!conveyorCache.ContainsKey(srcIndex))
            {
                conveyorCache[srcIndex] = new Dictionary<Direction, Array<string>>();
            }

            Array<string> connectionConveyorList = new Array<string>();
            for(int j = 1; j < completePath.Length - 1; j++)
            {
                string conveyorNodeName = $"Conveyor_{srcIndex}_{completePath[j]}";
                Direction inputDir = GetDirectionFromIndices(mapWidth, completePath[j], completePath[j-1]);
                Direction outputDir = GetDirectionFromIndices(mapWidth, completePath[j], completePath[j+1]);
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

        public static Direction GetDirectionFromIndices(int mapWidth, int selfIndex, int targetIndex)
        {
            int diff = targetIndex - selfIndex;
            if (diff == 1)
                return Direction.Right;

            if (diff == -1)
                return Direction.Left;

            if (diff == mapWidth)
                return Direction.Down;

            if (diff == -mapWidth)
                return Direction.Up;

            throw new System.Exception($"target {targetIndex} is not next to self {selfIndex}");
        }

        public static int GetIndicesFromDirection(int mapWidth, int selfIndex, Direction dir)
        {
            switch (dir)
            {
                case Direction.Right:
                    return selfIndex + 1;
                case Direction.Left:
                    return selfIndex - 1;
                case Direction.Down:
                    return selfIndex + mapWidth;
                case Direction.Up:
                    return selfIndex - mapWidth;
            }

            throw new System.Exception($"Invalid direction {dir}");
        }
    }
}