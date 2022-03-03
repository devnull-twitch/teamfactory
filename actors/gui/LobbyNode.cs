using Godot;
using TeamFactory.Lib.Multiplayer;

namespace TeamFactory.Gui
{
    public class LobbyNode : Node2D
    {
        public string ServerIP;

        public int ServerPort;

        private VBoxContainer userList;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            userList = GetNode<VBoxContainer>("/root/Lobby/CanvasLayer/UserList");

            LobbyServer server = new LobbyServer();
            server.Node = this;
            server.Name = "LobbyServer";
            AddChild(server);
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

        public void SwitchToGame()
        {
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
    }
}