using System.Text;
using Godot;
using TeamFactory.Util.Multiplayer;

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

            if (isQuickConnect())
            {
                GetNode<LineEdit>("../GameCodeInput").Text = "127.0.0.1:50127";
                CallDeferred("OnJoin");
            }

            HTTPRequest reqNode = GetNode<HTTPRequest>("HTTPRequest");
            reqNode.Connect("request_completed", this, nameof(OnJoinResult));
        }

        public void OnJoin()
        {
            string code = GetNode<LineEdit>("../GameCodeInput").Text;
            if (code.Contains(":"))
            {
                PackedScene lobbyPackged = GD.Load<PackedScene>("res://scenes/Lobby.tscn");
                GetNode<Node2D>("/root/Menu").QueueFree();

                LobbyNode lobbyNode = lobbyPackged.Instance<LobbyNode>();

                string[] parts = code.Split(":");
                lobbyNode.ServerIP = parts[0];
                lobbyNode.ServerPort = int.Parse(parts[1]);
                lobbyNode.LobbyCode = "Unknown";

                GetTree().Root.AddChild(lobbyNode);
            }
            else
            {
                HTTPRequest reqNode = GetNode<HTTPRequest>("HTTPRequest");
                Error err = reqNode.Request($"https://devnullga.me/team_factory/game?code={code}", null, true, HTTPClient.Method.Get);
                if (err != Error.Ok)
                {
                    GD.Print($"Error requesting lobby {err}");
                    return;
                }
            }
        }

        public void OnJoinResult(int result, int response_code, string[] headers, byte[] body)
        {
            if (response_code >= 400)
            {
                GD.Print("Error getting lobby");
                return;
            }

            string bodyStr = Encoding.UTF8.GetString(body);
            GD.Print(bodyStr);
            JSONParseResult json = JSON.Parse(bodyStr);
            Godot.Collections.Dictionary respData = (Godot.Collections.Dictionary)json.Result;

            if ((int)((System.Single)respData["port"]) == 0)
            {
                GD.Print("Error getting lobby");
                return;
            }

            PackedScene lobbyPackged = GD.Load<PackedScene>("res://scenes/Lobby.tscn");
            GetNode<Node2D>("/root/Menu").QueueFree();

            LobbyNode lobbyNode = lobbyPackged.Instance<LobbyNode>();
            
            lobbyNode.ServerIP = (string)respData["ip"];
            lobbyNode.ServerPort = (int)((System.Single)respData["port"]);
            lobbyNode.LobbyCode = GetNode<LineEdit>("../GameCodeInput").Text;

            GetTree().Root.AddChild(lobbyNode);
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