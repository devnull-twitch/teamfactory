using Godot;
using System;

namespace TeamFactory.Map 
{
    public class MapNode : Node2D
    {
        private MapServer server;

        private GridManager manager;

        public override void _Ready()
        {
            MapResource map = GD.Load<MapResource>("res://map/Starter.tres");

            manager = new GridManager(map, this);
            // TODO: if server
            manager.SetupMap();

            // TODO: if server
            server = new MapServer(this);
        }

        public override void _PhysicsProcess(float delta)
        {
        }
    }
}