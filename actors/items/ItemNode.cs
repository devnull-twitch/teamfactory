using Godot;
using System;
using TeamFactory.Lib.Multiplayer;
using TeamFactory.Map;

namespace TeamFactory.Items 
{
    public class ItemNode : Sprite
    {
        private ItemResource item;

        public Infra.InfraSprite Target;

        public Vector2[] Path;

        private ItemServer server;

        private ItemClient client;

        private NodeWrapper<ItemNode, ItemServer, ItemClient> multiplayerWrapper;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            server = new ItemServer(this);
            server.Path = Path;

            client = new ItemClient(this);
            multiplayerWrapper = new NodeWrapper<ItemNode, ItemServer, ItemClient>(this, server, client);

            ZIndex = ZIndex + 5;
        }

        public ItemResource Item
        {
            set{
                item = value;
                Texture = value.Texture;
            }
            get {
                return item;
            }
        }

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

        public override void _PhysicsProcess(float delta)
        {
            if (multiplayerWrapper != null)
            {
                multiplayerWrapper.Notification(NotificationPhysicsProcess, delta);
            }
        }

        private object[] shift(params object[] args)
        {
            object[] shifted = new object[args.Length - 1];
            System.Array.Copy(args, 1, shifted, 0, shifted.Length);
            return shifted;
        }
    }
}