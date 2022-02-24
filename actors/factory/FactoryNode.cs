using Godot;
using TeamFactory.Infra;
using TeamFactory.Items;
using TeamFactory.Lib.Multiplayer;

namespace TeamFactory.Factory
{
    public class FactoryNode : InfraSprite
    {
        private FactoryServer server;

        private FactoryClient client;

        private NodeWrapper<FactoryNode, FactoryServer, FactoryClient> multiplayerWrapper;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            server = new FactoryServer(this, TileRes);
            client = new FactoryClient(this);
            multiplayerWrapper = new NodeWrapper<FactoryNode, FactoryServer, FactoryClient>(this, server, client);
        }

        public override void _PhysicsProcess(float delta)
        {
            if (multiplayerWrapper != null)
            {
                multiplayerWrapper.Notification(NotificationPhysicsProcess, delta);
            }
            base._PhysicsProcess(delta);
        }

        // ClientSend provides access to the client to access the multiplayer wrapper
        public void ClientSend(string method, params object[] args)
        {
            multiplayerWrapper.ClientSend(method, args);
        }

        // ServerSend provides access to the server to access the multiplayer wrapper
        public void ServerSend(string method, params object[] args)
        {
            multiplayerWrapper.ServerSend(method, args);
        }

        // ServerRequest is called from network on the node and has to be relayed onto the client
        public void ServerRequest(params object[] args)
        {
            string method = (string)args[0];
            object[] restArgs = shift(args);
            client.ServerRequest(method, restArgs);
        }

        // ClientRequest is called from network on the node and has to be relayed onto the client
        public void ClientRequest(params object[] args)
        {
            string method = (string)args[0];
            object[] restArgs = shift(args);
            server.ClientRequest(method, restArgs);
        }

        public void SpawnItem()
        {
            PackedScene packedItemNode = GD.Load<PackedScene>("res://actors/items/Item.tscn");
            ItemNode newItemNode = packedItemNode.Instance<ItemNode>();
            newItemNode.Item = TileRes.SpawnResource;
            newItemNode.Path = GridManager.IndicesToWorld(TileRes.PathToTarget);
            newItemNode.Target = Target;
            AddChild(newItemNode);
            newItemNode.GlobalPosition = GlobalPosition;
        }

        public void ItemArrived(ItemNode itemNode)
        {
            server.ItemArrived(itemNode);
        }

        private object[] shift(params object[] args)
        {
            object[] shifted = new object[args.Length - 1];
            System.Array.Copy(args, 1, shifted, 0, shifted.Length);
            return shifted;
        }
    }
}