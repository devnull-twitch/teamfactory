using Godot;
using TeamFactory.Map;
using TeamFactory.Items;
using TeamFactory.Infra;
using TeamFactory.Lib.Multiplayer;

namespace TeamFactory.Input
{
    public class InputServer : Node
    {
        public InputNode Node;

        private float cooldown;

        public bool clientReady = false;

        private int itemCount;

        public override void _Ready()
        {
            if (NetState.Mode == Mode.NET_SERVER)
                return;

            NetState.RpcId(this, 1, "RequestClientReady");
        }

        public override void _PhysicsProcess(float delta)
        {
            if (NetState.Mode == Mode.NET_CLIENT)
            {
                return;
            }

            if (Node.OutConnections.Count <= 0)
            {
                return;
            }

            cooldown -= delta;
            if (cooldown <= 0)
            {
                cooldown = Node.SpawnInterval;
                NetState.Rpc(this, "SpawnItem", itemCount);
                itemCount++;
            }
        }

        public int GetTargetIndex()
        {
            foreach(System.Collections.Generic.KeyValuePair<GridManager.Direction, ConnectionTarget> tuple in Node.OutConnections)
            {
                return Node.GridManager.MapToIndex(tuple.Value.TargetCoords);
            }

            return -1;
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
            newItemNode.Name = $"Item_{itemID}";
            newItemNode.Item = Node.SpawnResource;
            newItemNode.GlobalPosition = Node.GlobalPosition;
            newItemNode.Target = targetNode;

            AddChild(newItemNode);
        }

        [Remote]
        public void RequestClientReady()
        {
            
        }
    }
}