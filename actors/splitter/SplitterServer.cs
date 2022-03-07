using Godot;
using TeamFactory.Items;
using TeamFactory.Infra;
using TeamFactory.Map;
using SysGen = System.Collections.Generic;

namespace TeamFactory.Splitter
{
    // TODO: splitter should have a storage for the case that an item arrives but no valid connection exists.
    
    public class SplitterServer : Node, IItemReceiver
    {
        public SplitterNode Node;

        private int currentTargetIndex;

        private int currentTargetMapIndex;

        public override void _Ready()
        {
            setNextTarget();
        }

        public void ItemArrived(ItemNode itemNode)
        {
            if (Node.TileRes.Connections.Count <= 0)
            {
                return;
            }

            PackedScene packedItemNode = GD.Load<PackedScene>("res://actors/items/Item.tscn");
            ItemNode newItemNode = packedItemNode.Instance<ItemNode>();
            newItemNode.Item = itemNode.Item;
            newItemNode.Path = Node.GridManager.IndicesToWorld(Node.TileRes.PathToTarget[currentTargetMapIndex]);
            newItemNode.Target = Node.Target;
            AddChild(newItemNode);
            newItemNode.GlobalPosition = Node.GlobalPosition;

            currentTargetIndex++;
            if (currentTargetIndex >= Node.TileRes.Connections.Count)
            {
                currentTargetIndex = 0;
            }
            
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
            if (Node.TileRes.Connections.Count <= 0)
            {
                return;
            }

            SysGen.IList<GridManager.Direction> keyList = SplitterServer.ConvertKeyCollection(Node.TileRes.Connections.Keys);
            Vector2 targetCoords = Node.TileRes.Connections[keyList[currentTargetIndex]].TargetCoords;
            int targetIndex = Node.GridManager.MapToIndex(targetCoords);
            currentTargetMapIndex = targetIndex;
            Node.Target = Node.GridManager.GetInfraAtIndex(targetIndex);
        }
    }
}
