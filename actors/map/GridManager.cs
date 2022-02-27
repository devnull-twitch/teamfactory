using Godot;
using TeamFactory.Conveyor;
using TeamFactory.Infra;
using Godot.Collections;

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

        private MapResource map;

        private AStar2D infraMap;

        private Node2D mapNode;

        private Dictionary<int, InfraSprite> infraCache = new Dictionary<int, InfraSprite>();

        public GridManager(MapResource map, Node2D mapNode)
        {
            this.map = map;
            this.mapNode = mapNode;
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

            src.Connections[outputDir] = new ConnectionTarget(target, Direction.Left);
            connectNode(src, target);
            return true;
        }

        public void SetupMap()
        {
            infraMap = new AStar2D();

            for (int i = 0; i < map.Tiles.Count; i++)
            {
                TileResource tr = map.Tiles[i];
                
                Vector2 mapPos = IndexToMap(i);
                infraMap.AddPoint(i, mapPos, 1);
                if (mapPos.x > 0) {
                    infraMap.ConnectPoints(i, i - 1, true);
                }
                if (mapPos.y > 0)
                {
                    infraMap.ConnectPoints(i, i - map.Width, true);
                }

                if (tr != null)
                {
                    addInfraNode(tr, i);
                }
            }

            for (int i = 0; i < map.Tiles.Count; i++)
            {
                TileResource tr = map.Tiles[i];
                if (tr != null && tr.Connections.Count > 0)
                {
                    connectNode(tr, i);
                }
            }
        }

        public int[] GetPathTo(int from, int to)
        {
            return infraMap.GetIdPath(from, to);
        }

        public Vector2 IndexToMap(int i)
        {
            int x = i % map.Width;
            int y = i / map.Width;

            return new Vector2(x, y);
        }

        public Vector2 IndexToWorld(int i)
        {
            float x = i % map.Width * CELLSIZE + (CELLSIZE / 2);
            float y = i / map.Width * CELLSIZE + (CELLSIZE / 2);

            return new Vector2(x, y);
        }

        public int WorldToIndex(Vector2 position)
        {
            int x = (int)position.x / CELLSIZE;
            int y = (int)position.y / CELLSIZE;

            return y * map.Width + x;
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
            tr.MapIndex = index;

            InfraSprite infraNode = tr.Infra.Instance<InfraSprite>();
            infraNode.Position = IndexToWorld(index);
            infraNode.RotateFromDirection(tr.Direction);
            infraNode.TileRes = tr;
            infraNode.GridManager = this;
            mapNode.AddChild(infraNode);
            infraMap.SetPointDisabled(index, true);

            infraCache[index] = infraNode;
        }

        private void connectNode(TileResource tr, int index)
        {
            foreach(System.Collections.Generic.KeyValuePair<Direction, ConnectionTarget> tuple in tr.Connections)
            {
                infraCache[index].Target = infraCache[tuple.Value.MapIndex];
                int startIndex = GetIndicesFromDirection(index, tuple.Key);
                int endIndex = GetIndicesFromDirection(tuple.Value.MapIndex, tuple.Value.Direction);

                // make path without source and dest
                int[] path = infraMap.GetIdPath(startIndex, endIndex);
                int[] completePath = new int[path.Length + 2];
                System.Array.Copy(path, 0, completePath, 1, path.Length);
                completePath[0] = index;
                // add in source and dest ( blocked in a star because they are infra )
                completePath[completePath.Length - 1] = tuple.Value.MapIndex;

                // save path in tile indices to target node
                tr.PathToTarget[tuple.Value.MapIndex] = completePath;

                PackedScene packedConveyor = GD.Load<PackedScene>("res://actors/conveyor/ConveyorNode.tscn");
                for(int j = 1; j < completePath.Length - 1; j++)
                {
                    int pathSegmentIndex = completePath[j];
                    int x = pathSegmentIndex % map.Width;
                    int y = pathSegmentIndex / map.Width;

                    ConveyorNode conveyorInstance = packedConveyor.Instance<ConveyorNode>();
                    conveyorInstance.Position = IndexToWorld(completePath[j]);
                    conveyorInstance.InputDir = GetDirectionFromIndices(completePath[j], completePath[j-1]);
                    conveyorInstance.OutputDir = GetDirectionFromIndices(completePath[j], completePath[j+1]);

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