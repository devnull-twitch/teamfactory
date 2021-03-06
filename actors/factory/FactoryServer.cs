using Godot;
using TeamFactory.Util.Multiplayer;
using TeamFactory.Map;
using TeamFactory.Items;
using TeamFactory.Infra;
using TeamFactory.Game;

namespace TeamFactory.Factory
{
    public class FactoryServer : Node, IItemReceiver
    {
        private float cooldown;

        public FactoryNode Node;

        private int itemCount;

        private GameServer gameServer;

        public override void _Ready()
        {
            gameServer = GetNode<GameServer>("/root/Game/GameServer");
        }

        public override void _PhysicsProcess(float delta)
        {
            if (NetState.Mode == Mode.NET_CLIENT)
                return;

            if (RequiredmentsCheck())
            {
                NetState.Rpc(Node, "SetWorkingFlag", true);
                cooldown -= delta;

                if (cooldown <= 0)
                {
                    gameServer.ReducePlayerPower(Node.OwnerID, Node.SpawnResource.PowerCost);
                    PopFromStorage();
                    cooldown = Node.SpawnInterval;

                    NetState.Rpc(this, "SpawnItem", itemCount);
                    itemCount++;
                }
            }
            else
            {
                NetState.Rpc(Node, "SetWorkingFlag", false);
            }
        }

        private bool RequiredmentsCheck()
        {
            if (Node.SpawnResource == null)
                return false;

            if (Node.Disabled)
                return false;

            if (Node.OutConnections.Count <= 0)
                return false;

            foreach(System.Collections.Generic.KeyValuePair<string, int> tuple in Node.SpawnResource.Requirements)
            {
                if (!Node.Storage.ContainsKey(tuple.Key))
                    return false;

                if (Node.Storage[tuple.Key] < tuple.Value)
                    return false;
            }

            if (gameServer.GetPlayerPower(Node.OwnerID) < Node.SpawnResource.PowerCost)
                return false;

            return true;
        }

        private void PopFromStorage()
        {
            foreach(System.Collections.Generic.KeyValuePair<string, int> tuple in Node.SpawnResource.Requirements)
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

            NetState.Rpc(Node, "SetWorkingFlag", RequiredmentsCheck());
        }

        [RemoteSync]
        public void StorageUpdate(string itemName, int newValue)
        {
            Node.Storage[itemName] = newValue;

            InfraWindow window = GetNodeOrNull<InfraWindow>("/root/Game/HUD/Panel");
            if (window != null && window.InfraNode == Node)
                window.UpdateWindow();
        }

        [RemoteSync]
        public void SpawnItem(int itemID)
        {
            int targetIndex = GetTargetIndex();
            if (targetIndex == -1)
                return;

            InfraSprite targetNode = Node.GridManager.GetInfraAtIndex(targetIndex);
            PackedScene packedItemNode = GD.Load<PackedScene>("res://actors/items/Item.tscn");
            ItemNode newItemNode = packedItemNode.Instance<ItemNode>();
            newItemNode.Item = Node.SpawnResource;
            newItemNode.Target = targetNode;
            newItemNode.Name = $"Item_{itemID}";
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