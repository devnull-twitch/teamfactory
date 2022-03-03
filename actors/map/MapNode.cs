using Godot;
using TeamFactory.Lib.Multiplayer;
using TeamFactory.Lib.JsonMap;

namespace TeamFactory.Map 
{
    public class MapNode : Node2D
    {
        private MapServer server;

        public GridManager Manager;

        public override void _Ready()
        {
            File testJson = new File();
            testJson.Open("res://map/testing.json", File.ModeFlags.Read);

            Parser parser = new Parser(testJson.GetAsText());

            Manager = new GridManager(this, parser);
            // TODO: if server
            Manager.AddPlayerZone(1337);
            Manager.AddPlayerZone(1338);

            // TODO: if server
            server = new MapServer(this);
        }

        public override void _PhysicsProcess(float delta)
        {
        }
    }
}