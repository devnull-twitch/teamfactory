using Godot;
using System;

namespace TeamFactory.Map 
{
    public class MapNode : Node2D
    {
        private MapServer server;

        public GridManager Manager;

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