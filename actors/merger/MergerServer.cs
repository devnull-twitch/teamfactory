using Godot;
using TeamFactory.Infra;
using TeamFactory.Items;
using TeamFactory.Map;

public class MergerServer : Node, IItemReceiver
{
    public MergerNode Node;

    public void ItemArrived(ItemNode itemNode)
    {
        PackedScene packedItemNode = GD.Load<PackedScene>("res://actors/items/Item.tscn");
        ItemNode newItemNode = packedItemNode.Instance<ItemNode>();
        newItemNode.Item = itemNode.Item;
        newItemNode.Path = Node.GridManager.IndicesToWorld(Node.TileRes.PathToTarget[GetTargetIndex()]);
        newItemNode.Target = Node.Target;
        AddChild(newItemNode);
        newItemNode.GlobalPosition = Node.GlobalPosition;
    }

    public int GetTargetIndex()
        {
            foreach(System.Collections.Generic.KeyValuePair<GridManager.Direction, ConnectionTarget> tuple in Node.TileRes.Connections)
            {
                return Node.GridManager.MapToIndex(tuple.Value.TargetCoords);
            }

            return -1;
        }
}
