using Godot;
using System;
using TeamFactory.Infra;
using TeamFactory.Items;

namespace TeamFactory.Input
{
    public class InputNode : InfraSprite
    {
        private InputServer server;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            server = new InputServer(this, TileRes);
        }

        public override void _PhysicsProcess(float delta)
        {
            server.Tick(delta);
        }
    }
}