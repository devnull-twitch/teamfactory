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

        public override void _Ready()
        {
            SysGen.IList<GridManager.Direction> keyList = SplitterServer.ConvertKeyCollection(Node.TileRes.Connections.Keys);
            int targetIndex = Node.TileRes.Connections[keyList[currentTargetIndex]];
            currentTargetMapIndex = targetIndex;
            Node.Target = Node.GridManager.GetInfraAtIndex(targetIndex);
        }

        public void ItemArrived(ItemNode itemNode)
        {
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
            
            SysGen.IList<GridManager.Direction> keyList = SplitterServer.ConvertKeyCollection(Node.TileRes.Connections.Keys);
            int targetIndex = Node.TileRes.Connections[keyList[currentTargetIndex]];
            currentTargetMapIndex = targetIndex;
            Node.Target = Node.GridManager.GetInfraAtIndex(targetIndex);
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
    }
}
