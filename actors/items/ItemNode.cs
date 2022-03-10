using Godot;
using System;
using TeamFactory.Lib.Multiplayer;
using TeamFactory.Map;

namespace TeamFactory.Items 
{
    public class ItemNode : Sprite
    {
        private ItemResource item;

        public Infra.InfraSprite Target;

        private ItemServer server;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            server = new ItemServer();
            server.Node = this;
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
    }
}