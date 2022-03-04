using Godot;
using TeamFactory.Lib.Multiplayer;
using TeamFactory.Map;
using TeamFactory.Items;
using TeamFactory.Infra;

namespace TeamFactory.Factory
{
    public class FactoryServer : Node, IItemReceiver
    {
        private float cooldown;

        public FactoryNode Node;

        public override void _PhysicsProcess(float delta)
        {
            if (NetState.Mode == Mode.NET_CLIENT)
            {
                return;
            }

            if (Node.TileRes.Connections.Count <= 0)
            {
                return;
            }

            cooldown -= delta;
            if (cooldown <= 0 && RequiredmentsCheck())
            {
                PopFromStorage();
                cooldown = Node.TileRes.SpawnInterval;

                NetState.Rpc(this, "SpawnItem");
            }
        }

        private bool RequiredmentsCheck()
        {
            if (Node.TileRes == null)
            {
                return false;
            }

            foreach(System.Collections.Generic.KeyValuePair<string, int> tuple in Node.TileRes.SpawnResource.Requirements)
            {
                if (!Node.Storage.ContainsKey(tuple.Key))
                {
                    return false;
                }

                if (Node.Storage[tuple.Key] < tuple.Value)
                {
                    return false;
                }
            }

            return true;
        }

        private void PopFromStorage()
        {
            foreach(System.Collections.Generic.KeyValuePair<string, int> tuple in Node.TileRes.SpawnResource.Requirements)
            {
                NetState.Rpc(this, "StorageUpdate", tuple.Key, Node.Storage[tuple.Key] - tuple.Value);
            }
        }

        public void ItemArrived(ItemNode itemNode)
        {
            int newVal = 1;
            if (Node.Storage.ContainsKey(itemNode.Item.Name)) {
                newVal = Node.Storage[itemNode.Item.Name] + 1;
            }
            NetState.Rpc(this, "StorageUpdate", itemNode.Item.Name, newVal);
        }

        [RemoteSync]
        public void StorageUpdate(string itemName, int newValue)
        {
            Node.Storage[itemName] = newValue;
        }

        [RemoteSync]
        public void SpawnItem()
        {
            PackedScene packedItemNode = GD.Load<PackedScene>("res://actors/items/Item.tscn");
            ItemNode newItemNode = packedItemNode.Instance<ItemNode>();
            newItemNode.Item = Node.TileRes.SpawnResource;
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
}