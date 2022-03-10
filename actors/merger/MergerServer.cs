using Godot;
using TeamFactory.Infra;
using TeamFactory.Items;
using TeamFactory.Map;

public class MergerServer : Node, IItemReceiver
{
    public MergerNode Node;

    public void ItemArrived(ItemNode itemNode)
    {
        int targetIndex = GetTargetIndex();
        if (targetIndex == -1)
            return;

        InfraSprite targetNode = Node.GridManager.GetInfraAtIndex(targetIndex);
        PackedScene packedItemNode = GD.Load<PackedScene>("res://actors/items/Item.tscn");
        ItemNode newItemNode = packedItemNode.Instance<ItemNode>();
        newItemNode.Item = itemNode.Item;
        newItemNode.Target = targetNode;
        AddChild(newItemNode);
        newItemNode.GlobalPosition = Node.GlobalPosition;
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
