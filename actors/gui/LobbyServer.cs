using Godot;
using Godot.Collections;
using TeamFactory.Lib.Multiplayer;
using TeamFactory.Game;

namespace TeamFactory.Gui
{
    public class LobbyServer : Node
    {
        public Array<string> Usernames = new Array<string>();

        public int playerIndex;

        private Dictionary<int, string> players = new Dictionary<int, string>();

        private Dictionary<int, bool> readyState = new Dictionary<int, bool>();

        public LobbyNode Node;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Usernames.Add("Connla");
            Usernames.Add("Lilith");
            Usernames.Add("Raghu");
            Usernames.Add("Krishna");
            Usernames.Add("Erna");
            Usernames.Add("Iacchus");
            Usernames.Add("Hadad");
            Usernames.Add("Lara");
            Usernames.Add("Flora");
            Usernames.Add("Halcyone");

            if (NetState.Mode == Mode.LOCAL)
            {
                onNetworkPeerConnected(NetState.NetworkId(Node));
            }

            string[] args = OS.GetCmdlineArgs(); 
            for (int key = 0; key < args.Length; ++key)
            {
                string arg = args[key];
                if (arg == "--server")
                {
                    NetState.Mode = Mode.NET_SERVER;
                    int port = int.Parse(args[key+1]);
                    startServer(port);
                    return;
                }
            }
        }

        protected void startServer(int port)
        {
            GetTree().Connect("network_peer_connected", this, nameof(onNetworkPeerConnected));
            GetTree().Connect("network_peer_disconnected", this, nameof(onNetworkPeerDisconnected));

            GD.PrintS($"Starting Server on port {port}!\n");

            NetworkedMultiplayerENet peer = new NetworkedMultiplayerENet();
            peer.ServerRelay = false;
            var error = peer.CreateServer(port);
            if (error != Error.Ok) 
            {
                GD.PrintErr(error);
                return;
            }

            GetTree().NetworkPeer = peer;
        }

        public void onNetworkPeerConnected(int id)
        {
            string newPlayername = Usernames[playerIndex];
            playerIndex++;

            players[id] = newPlayername;
            readyState[id] = false;

            NetState.Rpc(Node, "AddPlayerName", newPlayername);
        }

        public void onNetworkPeerDisconnected(int id)
        {
            NetState.Rpc(Node, "RemovePlayerName", players[id]);
            players.Remove(id);
        }

        [Remote]
        public void PlayerRequestStart()
        {
            int id = NetState.NetworkSenderId(Node);
            readyState[id] = true;

            if (NetState.Mode != Mode.LOCAL)
            {
                /*
                if (readyState.Count < 2)
                {
                    return;
                }
                */
                foreach(bool readyFlag in readyState.Values)
                {
                    if (!readyFlag)
                    {
                        return;
                    }
                }
            }
            
            GetTree().RefuseNewNetworkConnections = true;

            PackedScene gamePacked = GD.Load<PackedScene>("res://scenes/Game.tscn");
            GetNode<Node2D>("/root/Lobby").QueueFree();

            GameNode gameNode = gamePacked.Instance<GameNode>();
            GameServer gameServerNode = gameNode.GetNode<GameServer>("GameServer");
            foreach(int netID in players.Keys)
            {
                gameServerNode.AddPlayer(netID, players[netID]);
            }
            GetTree().Root.AddChild(gameNode);

            NetState.Rpc(Node, "SwitchToGame");
        }
    }
}