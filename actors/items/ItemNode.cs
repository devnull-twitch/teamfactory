using Godot;
using System;
using TeamFactory.Util.Multiplayer;
using TeamFactory.Map;

namespace TeamFactory.Items 
{
    public class ItemNode : Sprite
    {
        private ItemResource item;

        public Infra.InfraSprite Target;

        private ItemServer server;

        public Vector2? NextStep;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            server = new ItemServer();
            server.Node = this;
            server.Name = "ItemServer";
            AddChild(server);

            ZIndex = ZIndex + 5;
        }

        public ItemResource Item
        {
            set{
                item = value;
                Texture = value.Texture;
            }
            get {
                return item;
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            if (NextStep == null)
                return;

            float totalDistance = GlobalPosition.DistanceTo((Vector2)NextStep);
            if(totalDistance < 2)
            {
                NextStep = null;
                return;
            }

            GlobalPosition = GlobalPosition.MoveToward((Vector2)NextStep, 200 * delta);
        }
    }
}