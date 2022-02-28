using Godot;
using TeamFactory.Lib.Multiplayer;
using TeamFactory.Lib.JsonMap;

namespace TeamFactory.Map 
{
    public class MapNode : Node2D
    {
        private MapServer server;

        public GridManager Manager;

        public MapNode() : base()
        {
            // TODO: read from somewhere
            NetState.Mode = Mode.LOCAL;
        }

        public override void _Ready()
        {
            File testJson = new File();
            testJson.Open("res://map/testing.json", File.ModeFlags.Read);

            Parser parser = new Parser();
            MapResource mapResource = parser.ParseAsMap(testJson.GetAsText());

            PackedScene packedFloor = GD.Load<PackedScene>("res://actors/floor/Floor.tscn");
            TileMap floor = packedFloor.Instance<TileMap>();
            for (int x = 0; x < mapResource.Width; x++)
            {
                floor.SetCell(x, -1, 1, false, false, true);
                for (int y = 0; y < mapResource.Height; y++)
                {
                    if (x == 0)
                    {
                        floor.SetCell(-1, y, 1);
                    }
                    floor.SetCell(x, y, 0);
                    if (x == mapResource.Width - 1)
                    {
                        floor.SetCell(mapResource.Width, y, 1);
                    }
                }
                floor.SetCell(x, mapResource.Height, 1, false, false, true);
            }
            AddChild(floor);

            Manager = new GridManager(mapResource, this);
            // TODO: if server
            Manager.SetupMap();

            // TODO: if server
            server = new MapServer(this);
        }

        public override void _PhysicsProcess(float delta)
        {
        }
    }
}