using Godot;
using System;
using TeamFactory.Map;
using TeamFactory.Items;

namespace TeamFactory.Input
{
    public class InputServer
    {
        private InputNode node;

        private TileResource tileResource;

        private float cooldown;

        public InputServer(InputNode node, TileResource tileResource)
        {
            this.node = node;
            this.tileResource = tileResource;

            cooldown = tileResource.SpawnInterval;
        }

        public void Tick(float delta)
        {
            cooldown -= delta;
            if (cooldown <= 0)
            {
                cooldown = tileResource.SpawnInterval;

                PackedScene packedItemNode = GD.Load<PackedScene>("res://actors/items/Item.tscn");
                ItemNode newItemNode = packedItemNode.Instance<ItemNode>();
                newItemNode.Item = tileResource.SpawnResource;
                newItemNode.GlobalPosition = node.GlobalPosition;
                newItemNode.Path = node.GridManager.IndicesToWorld(tileResource.PathToTarget);
                newItemNode.Target = node.Target;

                node.GetNode<Node>("/root/Game/Items").AddChild(newItemNode);
            }
        }
    }
}