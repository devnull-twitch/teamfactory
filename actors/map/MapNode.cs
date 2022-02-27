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