using Godot;
using System;

namespace TeamFactory.Map 
{
    public class MapNode : TileMap
    {
        [Export]
        public MapResource CurrentMap;

        private MapServer server;

        protected void testSetup()
        {
            MapResource map = GD.Load<MapResource>("res://map/Starter.tres");
            map.Width = 15;

            TileResource inputNode = new TileResource();
            inputNode.TeamID = 1;
            inputNode.InfraPlaced = 0;
            inputNode.ConnectedTo = 15*4+2;
            inputNode.SpawnResource = GD.Load<Items.ItemResource>("res://actors/items/ironore/Ironore.tres");
            inputNode.SpawnInterval = 1;
            inputNode.TimeToNextSpawn = 1;
            map.Tiles[15*4] = inputNode;

            TileResource factoryNode = new TileResource();
            factoryNode.TeamID = 1;
            factoryNode.InfraPlaced = 2;
            factoryNode.ConnectedTo = 15*5+6;
            factoryNode.SpawnResource = GD.Load<Items.ItemResource>("res://actors/items/ironbar/Ironbar.tres");
            factoryNode.Requirements = new Godot.Collections.Dictionary<string, int>();
            factoryNode.Requirements.Add("Ironore", 2);
            map.Tiles[15*4+2] = factoryNode;

            TileResource outputNode = new TileResource();
            outputNode.TeamID = 1;
            outputNode.InfraPlaced = 1;
            outputNode.FlipX = true;
            outputNode.IsFinal = true;
            map.Tiles[15*5+6] = outputNode;

            CurrentMap = map;
        }

        public override void _Ready()
        {
            testSetup();

            // TODO: if server
            server = new MapServer(this);

            AStar2D infraMap = new AStar2D();

            for (int i = 0; i < CurrentMap.Tiles.Count; i++)
            {
                TileResource tr = CurrentMap.Tiles[i];
                int x = i % CurrentMap.Width;
                int y = i / CurrentMap.Width;
                
                infraMap.AddPoint(i, new Vector2(x, y), 1);
                if (x > 0) {
                    infraMap.ConnectPoints(i, i - 1, true);
                }
                if (y > 0)
                {
                    infraMap.ConnectPoints(i, i - CurrentMap.Width, true);
                }

                if (tr != null)
                {
                    SetCell(x, y, tr.InfraPlaced, tr.FlipX, tr.FlipY);
                    infraMap.SetPointDisabled(i, true);
                }
            }

            for (int i = 0; i < CurrentMap.Tiles.Count; i++)
            {
                TileResource tr = CurrentMap.Tiles[i];
                if (tr != null && tr.ConnectedTo != -1)
                {
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
                        int x = pathSegmentIndex % CurrentMap.Width;
                        int y = pathSegmentIndex / CurrentMap.Width;

                        int tileID = 5;
                        bool flipx = false;
                        bool flipy = false;

                        int inDir = completePath[j] - completePath[j-1];
                        int outDir = completePath[j+1] - completePath[j];
                        if (inDir == 1 && outDir == CurrentMap.Width)
                        {
                            tileID = 3;
                        }
                        if (inDir == 1 && outDir == -CurrentMap.Width)
                        {
                            tileID = 4;
                        }

                        if (inDir == 15 && outDir == 1)
                        {
                            flipx = true;
                            flipy = true;
                            tileID = 4;
                        }

                        SetCell(x, y, tileID, flipx, flipy);
                    }
                }
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            // TODO: if server
            server.Tick(delta);
        }

        public Vector2 IndexToMap(int i)
        {
            int x = i % CurrentMap.Width;
            int y = i / CurrentMap.Width;

            return new Vector2(x, y);
        }

        public Vector2 IndexToWorld(int i)
        {
            float x = i % CurrentMap.Width * CellSize.x + (CellSize.x / 2);
            float y = i / CurrentMap.Width * CellSize.y + (CellSize.y / 2);

            return new Vector2(x, y);
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