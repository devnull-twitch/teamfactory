using Godot;
using TeamFactory.Lib.Multiplayer;

namespace TeamFactory.Gui
{
    public class JoinBtn : Button
    {
        public override void _Ready()
        {
            File netFlagFile = new File();
            if (netFlagFile.FileExists("res://force_local.txt"))
            {
                NetState.Mode = Mode.LOCAL;
                GD.Print("Using local mode");
            }

            Connect("pressed", this, nameof(OnJoin));
        }

        public void OnJoin()
        {
            string code = GetNode<LineEdit>("../GameCodeInput").Text;
            
            // make some http request to check if code exists and get IP & port

            PackedScene lobbyPackged = GD.Load<PackedScene>("res://scenes/Lobby.tscn");
            GetNode<Node2D>("/root/Menu").QueueFree();

            LobbyNode lobbyNode = lobbyPackged.Instance<LobbyNode>();
            
            string codeInput = GetNode<LineEdit>("../GameCodeInput").Text;
            if (codeInput.Contains(":"))
            {
                string[] parts = codeInput.Split(":");
                lobbyNode.ServerIP = parts[0];
                lobbyNode.ServerPort = int.Parse(parts[1]);
            }

            GetTree().Root.AddChild(lobbyNode);
        }
    }
}