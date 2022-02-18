using Godot;
using TeamFactory.Items;
using System.Collections.Generic;

namespace TeamFactory.Map
{
    public class ArrivalCallback
    {
        public ArrivalCallback(TileResource target, MapServer server)
        {
            TargetTileResource = target;
            Server = server;
        }

        public TileResource TargetTileResource;

        public MapServer Server;

        public void ItemArrived(ItemResource itemRes)
        {
            if (TargetTileResource.IsFinal)
            {
                GD.Print("should add points");
                return;
            }

            if (!TargetTileResource.Storage.ContainsKey(itemRes.Name))
            {
                TargetTileResource.Storage.Add(itemRes.Name, 1);
                return;
            }

            TargetTileResource.Storage[itemRes.Name] += 1;
        }
    }

    public class MapServer
    {
        protected MapNode node;

        public MapServer(MapNode node)
        {
            this.node = node;
        }

        public void Tick(float delta)
        {
            for (int i = 0; i < node.CurrentMap.Tiles.Count; i++)
            {
                TileResource tr = node.CurrentMap.Tiles[i];
                if (tr != null && tr.ConnectedTo != -1 && tr.SpawnResource != null)
                {
                    tr.TimeToNextSpawn -= delta;

                    if (tr.IsReadyForSpawn)
                    {
                        Vector2 itemSpawn = node.IndexToWorld(i);
                        Vector2[] path = node.IndicesToWorld(tr.PathToTarget);
                        SpawnItem(tr.SpawnResource, itemSpawn, path, node.CurrentMap.Tiles[tr.ConnectedTo]);
                        tr.PostSpawn();
                    }
                }
            }
        }

        public void SpawnItem(ItemResource spawnItem, Vector2 pos, Vector2[] path, TileResource targetTR)
        {
            PackedScene packedItemNode = GD.Load<PackedScene>("res://actors/items/Item.tscn");
            ItemNode newItemNode = packedItemNode.Instance<ItemNode>();
            newItemNode.Item = spawnItem;
            newItemNode.Position = pos;
            newItemNode.Path = path;
            newItemNode.Callback = new ArrivalCallback(targetTR, this);

            node.GetNode<Node>("/root/Game/Items").AddChild(newItemNode);
        }
    }
}