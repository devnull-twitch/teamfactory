using Godot;
using TeamFactory.Items;
using TeamFactory.Infra;
using TeamFactory.Map;
using SysGen = System.Collections.Generic;

namespace TeamFactory.Splitter
{
    public class SplitterServer : Node, IItemReceiver
    {
        public SplitterNode Node;

        private int currentTargetIndex;

        private int currentTargetMapIndex;

        private InfraSprite targetNode;

        public override void _Ready()
        {
            setNextTarget();
        }

        public void ItemArrived(ItemNode itemNode)
        {
            if (targetNode == null)
            {
                return;
            }

            PackedScene packedItemNode = GD.Load<PackedScene>("res://actors/items/Item.tscn");
            ItemNode newItemNode = packedItemNode.Instance<ItemNode>();
            newItemNode.Item = itemNode.Item;
            newItemNode.Target = targetNode;
            AddChild(newItemNode);
            newItemNode.GlobalPosition = Node.GlobalPosition;

            setNextTarget();
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

        private void setNextTarget()
        {
            targetNode = null;

            if (Node.OutConnections.Count <= 0)
                return;
            
            SysGen.IList<GridManager.Direction> keyList = SplitterServer.ConvertKeyCollection(Node.OutConnections.Keys);
            Vector2 targetCoords = Node.OutConnections[keyList[currentTargetIndex]].TargetCoords;
            int targetIndex = Node.GridManager.MapToIndex(targetCoords);
            currentTargetMapIndex = targetIndex;
            targetNode = Node.GridManager.GetInfraAtIndex(targetIndex);

            currentTargetIndex++;
            if (currentTargetIndex >= Node.OutConnections.Count)
            {
                currentTargetIndex = 0;
            }
        }
    }
}
