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
                    InfraSprite infraNode = tr.Infra.Instance<InfraSprite>();
                    infraNode.Position = IndexToWorld(i);
                    infraNode.RotateFromDirection(tr.Direction);
                    infraNode.TileRes = tr;
                    infraNode.GridManager = this;
                    mapNode.AddChild(infraNode);
                    infraMap.SetPointDisabled(i, true);

                    infraCache[i] = infraNode;
                }
            }

            PackedScene packedConveyor = GD.Load<PackedScene>("res://actors/conveyor/ConveyorNode.tscn");
            for (int i = 0; i < map.Tiles.Count; i++)
            {
                TileResource tr = map.Tiles[i];
                if (tr != null && tr.ConnectedTo != -1)
                {
                    infraCache[i].Target = infraCache[tr.ConnectedTo];

                    // make path without source and dest
                    int[] path = infraMap.GetIdPath(i + 1, tr.ConnectedTo - 1);
                    int[] completePath = new int[path.Length + 2];
                    System.Array.Copy(path, 0, completePath, 1, path.Length);
                    completePath[0] = i;
                    // add in source and dest ( blocked in a star because they are infra )
                    completePath[completePath.Length - 1] = tr.ConnectedTo;

                    // save path in tile indices to target node
                    tr.PathToTarget = completePath;

                    for(int j = 1; j < completePath.Length - 1; j++)
                    {
                        int pathSegmentIndex = completePath[j];
                        int x = pathSegmentIndex % map.Width;
                        int y = pathSegmentIndex / map.Width;

                        ConveyorNode conveyorInstance = packedConveyor.Instance<ConveyorNode>();
                        conveyorInstance.Position = IndexToWorld(completePath[j]);
                        conveyorInstance.InputDir = ConveyorNode.GetDirectionFromIndices(completePath[j], completePath[j-1]);
                        conveyorInstance.OutputDir = ConveyorNode.GetDirectionFromIndices(completePath[j], completePath[j+1]);

                        mapNode.AddChild(conveyorInstance);
                    }
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
    }
}