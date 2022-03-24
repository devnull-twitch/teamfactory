using Godot;
using TeamFactory.Infra;
using TeamFactory.Items;
using TeamFactory.Map;
using TeamFactory.Util.Multiplayer;

namespace TeamFactory.Merger
{
    public class MergerServer : Node, IItemReceiver
    {
        public MergerNode Node;

        private int itemCount;

        public void ItemArrived(ItemNode itemNode)
        {
            int targetIndex = GetTargetIndex();
            if (targetIndex == -1)
                return;

            NetState.Rpc(this, "SpawnItem", itemCount, itemNode.Item.Name);
            itemCount++;
        }

        [RemoteSync]
        public void SpawnItem(int itemCount, string itemName)
        {
            ItemDB itemDB = GD.Load<ItemDB>("res://actors/items/ItemDB.tres");
            ItemResource relayedItem = itemDB.Database[itemName];

            int targetIndex = GetTargetIndex();
            if (targetIndex == -1)
                return;

            InfraSprite targetNode = Node.GridManager.GetInfraAtIndex(targetIndex);
            PackedScene packedItemNode = GD.Load<PackedScene>("res://actors/items/Item.tscn");
            ItemNode newItemNode = packedItemNode.Instance<ItemNode>();
            newItemNode.Item = relayedItem;
            newItemNode.Target = targetNode;
            newItemNode.Name = $"Item_{itemCount}";
            newItemNode.GlobalPosition = Node.GlobalPosition;

            AddChild(newItemNode);
        }

        public int GetTargetIndex()
        {
            foreach(System.Collections.Generic.KeyValuePair<GridManager.Direction, ConnectionTarget> tuple in Node.OutConnections)
            {
                return Node.GridManager.MapToIndex(tuple.Value.TargetCoords);
            }

            return -1;
        }
    }
}