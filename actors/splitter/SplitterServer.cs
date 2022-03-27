using Godot;
using TeamFactory.Items;
using TeamFactory.Infra;
using TeamFactory.Map;
using SysGen = System.Collections.Generic;
using TeamFactory.Util.Multiplayer;

namespace TeamFactory.Splitter
{
    public class SplitterServer : Node, IItemReceiver
    {
        public SplitterNode Node;

        private int currentTargetIndex;

        private int currentTargetMapIndex;

        private InfraSprite targetNode;

        private int itemCount;

        public override void _Ready()
        {
            if (NetState.Mode != Mode.NET_CLIENT)
                SetNextTarget();
        }

        public void ItemArrived(ItemNode itemNode)
        {
            if (targetNode == null)
                return;

            int targetIndex = Node.GridManager.WorldToIndex(targetNode.GlobalPosition);
            NetState.Rpc(this, "SpawnItem", itemCount, itemNode.Item.Name, targetIndex);
            itemCount++;

            SetNextTarget();
        }

        [RemoteSync]
        public void SpawnItem(int itemCount, string itemName, int targetIndex)
        {
            ItemDB itemDB = GD.Load<ItemDB>("res://actors/items/ItemDB.tres");
            ItemResource relayedItem = itemDB.Database[itemName];

            InfraSprite targetNode = Node.GridManager.GetInfraAtIndex(targetIndex);
            PackedScene packedItemNode = GD.Load<PackedScene>("res://actors/items/Item.tscn");
            ItemNode newItemNode = packedItemNode.Instance<ItemNode>();
            newItemNode.Item = relayedItem;
            newItemNode.Target = targetNode;
            newItemNode.Name = $"Item_{itemCount}";
            newItemNode.GlobalPosition = Node.GlobalPosition;

            AddChild(newItemNode);
        }

        public static SysGen.IList<GridManager.Direction> ConvertKeyCollection(SysGen.ICollection<GridManager.Direction> keys)
        {
            SysGen.List<GridManager.Direction> res = new SysGen.List<GridManager.Direction>();
            foreach(GridManager.Direction key in keys)
            {
                res.Add(key);
            }
            return res;
        }

        public void SetNextTarget()
        {
            targetNode = null;

            if (Node.OutConnections.Count <= 0)
                return;

            if (currentTargetIndex >= Node.OutConnections.Count)
                currentTargetIndex = 0;
            
            SysGen.IList<GridManager.Direction> keyList = SplitterServer.ConvertKeyCollection(Node.OutConnections.Keys);
            Vector2 targetCoords = Node.OutConnections[keyList[currentTargetIndex]].TargetCoords;
            int targetIndex = Node.GridManager.MapToIndex(targetCoords);
            currentTargetMapIndex = targetIndex;
            targetNode = Node.GridManager.GetInfraAtIndex(targetIndex);

            currentTargetIndex++;
        }
    }
}
