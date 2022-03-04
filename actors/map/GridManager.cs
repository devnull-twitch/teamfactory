using Godot;
using TeamFactory.Conveyor;
using TeamFactory.Infra;
using TeamFactory.Factory;
using Godot.Collections;
using TeamFactory.Lib.JsonMap;

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

        private Dictionary<int, InfraSprite> infraCache = new Dictionary<int, InfraSprite>();

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
            return infraCache[index];
        }

        public bool ConnectTileResource(TileResource src, int target, Direction outputDir)
        {
            if (target > infraCache.Count - 1)
            {
                GD.Print($"target out of bound {target}");
                return false;
            }

            if (infraCache[target] == null)
            {
                GD.Print($"target is empty {target}");
                return false;
            }

            Vector2 targetMapCoords = IndexToMap(target);
            src.Connections[outputDir] = new ConnectionTarget(targetMapCoords, Direction.Left);
            connectNode(src, target);
            return true;
        }

        public void Cleanup()
        {
            infraMap = new AStar2D();
            infraCache = new Dictionary<int, InfraSprite>();

            foreach(Node n in mapNode.GetChildren())
            {
                n.QueueFree();
            }

            TileMap floor = mapNode.GetNode<TileMap>("../Floor");
            floor.Clear();
        }

        public void AddPlayerZone(int ownerNetID, string color = "#EEEEEE")
        {
            MapResource map = Parser.CreateMapData();
            mapWidth = map.Width;
            mapHeight = map.Height;

            addPlayerFloor(map, color);

            // moving player to factory
            int relSpawnPosIndex = MapToIndex(map.SpawnPosition);
            int absoluteSpawnPosIndex = offset + relSpawnPosIndex;
            Vector2 spawnPos = IndexToWorld(absoluteSpawnPosIndex);
            mapNode.GetNode<Node2D>($"../Players/{ownerNetID}").Position = spawnPos;

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

            offset += mapWidth * (mapHeight + 1);
        }

        private void addPlayerFloor(MapResource map, string color)
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

            Vector2 offsetCoords = IndexToMap(offset);

            for (int x = 0; x < map.Width; x++)
            {
                floor.SetCell((int)offsetCoords.x + x, (int)offsetCoords.y - 1, 1, false, false, true);
                for (int y = 0; y < map.Height; y++)
                {
                    if (x == 0)
                    {
                        floor.SetCell((int)offsetCoords.x - 1, (int)offsetCoords.y + y, 1);
                    }
                    floor.SetCell((int)offsetCoords.x + x, (int)offsetCoords.y + y, newTileID);
                    if (x == map.Width - 1)
                    {
                        floor.SetCell((int)offsetCoords.x + map.Width, (int)offsetCoords.y + y, 1);
                    }
                }
                floor.SetCell((int)offsetCoords.x + x, (int)offsetCoords.y + map.Height, 1, false, false, true);
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
            InfraSprite infraNode = tr.Infra.Instance<InfraSprite>();
            infraNode.Position = IndexToWorld(index);
            infraNode.RotateFromDirection(tr.Direction);
            infraNode.TileRes = tr;
            infraNode.GridManager = this;
            infraMap.SetPointDisabled(index, true);

            if (tr.InfraOptions != null)
            {
                if (infraNode is FactoryNode factoryNode && tr.InfraOptions.ContainsKey("is_multi") && tr.InfraOptions["is_multi"] == "1")
                {
                    factoryNode.IsMulti = true;
                }
            }

            foreach(Direction inputDir in tr.Inputs)
            {
                int inputIndex = GetIndicesFromDirection(index, inputDir);
                infraMap.SetPointWeightScale(inputIndex, 3f);
            }

            foreach(Direction outputDir in tr.Connections.Keys)
            {
                int inputIndex = GetIndicesFromDirection(index, outputDir);
                infraMap.SetPointWeightScale(inputIndex, 3f);
            }

            mapNode.AddChild(infraNode);

            infraCache[index] = infraNode;
        }

        private void connectNode(TileResource tr, int index)
        {
            foreach(System.Collections.Generic.KeyValuePair<Direction, ConnectionTarget> tuple in tr.Connections)
            {
                int targetAbsIndex = MapToIndex(tuple.Value.TargetCoords);
                infraCache[index].Target = infraCache[targetAbsIndex];
                int startIndex = GetIndicesFromDirection(index, tuple.Key);
                int endIndex = GetIndicesFromDirection(targetAbsIndex, tuple.Value.Direction);

                // make path without source and dest
                int[] path = infraMap.GetIdPath(startIndex, endIndex);
                int[] completePath = new int[path.Length + 2];
                System.Array.Copy(path, 0, completePath, 1, path.Length);
                completePath[0] = index;
                // add in source and dest ( blocked in a star because they are infra )
                completePath[completePath.Length - 1] = targetAbsIndex;

                // save path in tile indices to target node
                tr.PathToTarget[targetAbsIndex] = completePath;

                PackedScene packedConveyor = GD.Load<PackedScene>("res://actors/conveyor/ConveyorNode.tscn");
                for(int j = 1; j < completePath.Length - 1; j++)
                {
                    int pathSegmentIndex = completePath[j];
                    int x = pathSegmentIndex % mapWidth;
                    int y = pathSegmentIndex / mapWidth;

                    ConveyorNode conveyorInstance = packedConveyor.Instance<ConveyorNode>();
                    conveyorInstance.Position = IndexToWorld(completePath[j]);
                    conveyorInstance.InputDir = GetDirectionFromIndices(completePath[j], completePath[j-1]);
                    conveyorInstance.OutputDir = GetDirectionFromIndices(completePath[j], completePath[j+1]);

                    infraMap.SetPointWeightScale(completePath[j], 3f);

                    mapNode.AddChild(conveyorInstance);
                }
            }
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