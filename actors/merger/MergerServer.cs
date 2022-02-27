using Godot;
using TeamFactory.Infra;
using TeamFactory.Items;

public class MergerServer : Node, IItemReceiver
{
    public MergerNode Node;

    public void ItemArrived(ItemNode itemNode)
    {
        PackedScene packedItemNode = GD.Load<PackedScene>("res://actors/items/Item.tscn");
        ItemNode newItemNode = packedItemNode.Instance<ItemNode>();
        newItemNode.Item = itemNode.Item;
        newItemNode.Path = Node.GridManager.IndicesToWorld(Node.TileRes.PathToTarget[0]);
        newItemNode.Target = Node.Target;
        AddChild(newItemNode);
        newItemNode.GlobalPosition = Node.GlobalPosition;
    }
}
