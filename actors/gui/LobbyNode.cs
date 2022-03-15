using Godot;
using TeamFactory.Util.Multiplayer;

namespace TeamFactory.Gui
{
    public class LobbyNode : Node2D
    {
        public string ServerIP;

        public int ServerPort;

        private VBoxContainer userList;

        private LobbyServer server;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            userList = GetNode<VBoxContainer>("/root/Lobby/CanvasLayer/UserList");

            server = new LobbyServer();
            server.Node = this;
            server.Name = "LobbyServer";
            AddChild(server);

            if (NetState.Mode == Mode.NET_CLIENT)
            {
                setupNetClient();
            }
        }

        [Remote]
        public void AddPlayerName(string name)
        {
            Label playerNameLabel = new Label();
            playerNameLabel.Text = name;
            userList.AddChild(playerNameLabel);
            userList.MoveChild(playerNameLabel, 0);
        }

        [Remote]
        public void RemovePlayerName(string name)
        {
            foreach (Control n in userList.GetChildren())
            {
                if (n is Label labelNode && labelNode.Text == name)
                {
                    labelNode.QueueFree();
                }
            }
        }

        [Remote]
        public void SwitchToGame()
        {
            GD.Print("SwitchToGame");
            if(NetState.Mode != Mode.NET_CLIENT)
            {
                return;
            }

            PackedScene gamePacked = GD.Load<PackedScene>("res://scenes/Game.tscn");
            GetNode<Node2D>("/root/Lobby").QueueFree();

            Node gameNode = gamePacked.Instance();
            // copy over player ID to name dictionary
            GetTree().Root.AddChild(gameNode);
        }

        private void setupNetClient()
        {
            GD.PrintS("Starting Client!\n");

            NetworkedMultiplayerENet peer = new NetworkedMultiplayerENet();
            var error = peer.CreateClient(ServerIP, ServerPort);
            if (error != Error.Ok) 
            {
                GD.PrintErr(error);
                return;
            }

            GetTree().NetworkPeer = peer;

            GetTree().Connect("connected_to_server", this, nameof(OnConnectedToServer));
        }

        public void OnConnectedToServer()
        {
            if (isQuickConnect())
            {
                NetState.RpcId(server, 1, "PlayerRequestStart");
            }
        }

        private bool isQuickConnect()
        {
            string[] args = OS.GetCmdlineArgs(); 
            for (int key = 0; key < args.Length; ++key)
            {
                string arg = args[key];
                if (arg == "--quickconnect")
                {
                    return true;
                }
            }

            return false;
        }
    }
}