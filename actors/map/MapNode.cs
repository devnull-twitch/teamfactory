using Godot;
using TeamFactory.Lib.Multiplayer;

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
            MapResource map = GD.Load<MapResource>("res://map/Starter.tres");

            Manager = new GridManager(map, this);
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