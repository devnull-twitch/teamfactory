using Godot;
using System;
using TeamFactory.Map;
using TeamFactory.Items;

namespace TeamFactory.Input
{
    public class InputServer : Node
    {
        public InputNode Node;

        private float cooldown;

        public override void _PhysicsProcess(float delta)
        {
            cooldown -= delta;
            if (cooldown <= 0)
            {
                cooldown = Node.TileRes.SpawnInterval;

                PackedScene packedItemNode = GD.Load<PackedScene>("res://actors/items/Item.tscn");
                ItemNode newItemNode = packedItemNode.Instance<ItemNode>();
                newItemNode.Item = Node.TileRes.SpawnResource;
                newItemNode.GlobalPosition = Node.GlobalPosition;
                newItemNode.Path = Node.GridManager.IndicesToWorld(Node.TileRes.PathToTarget[GetTargetIndex()]);
                newItemNode.Target = Node.Target;

                AddChild(newItemNode);
            }
        }

        public int GetTargetIndex()
        {
            foreach(System.Collections.Generic.KeyValuePair<GridManager.Direction, int> tuple in Node.TileRes.Connections)
            {
                return tuple.Value;
            }

            return -1;
        }
    }
}