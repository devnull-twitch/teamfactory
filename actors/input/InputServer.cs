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
            if (cooldown <= 0 && tileResource.IsReadyForSpawn)
            {
                tileResource.PostSpawn();
                cooldown = tileResource.SpawnInterval;

                PackedScene packedItemNode = GD.Load<PackedScene>("res://actors/items/Item.tscn");
                ItemNode newItemNode = packedItemNode.Instance<ItemNode>();
                newItemNode.Item = tileResource.SpawnResource;
                newItemNode.Position = node.Position;
                newItemNode.Path = node.GridManager.IndicesToWorld(tileResource.PathToTarget);
                newItemNode.OwnerNode = node;
                
                if (node.Target != null) 
                {
                    newItemNode.TargetNode = node.Target;
                }

                node.GetNode<Node>("/root/Game/Items").AddChild(newItemNode);
            }
        }
    }
}