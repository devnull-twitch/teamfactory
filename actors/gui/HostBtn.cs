using Godot;
using System.Text;
using System.Collections.Generic;

namespace TeamFactory.Gui
{
    public class HostBtn : Button
    {
        public override void _Ready()
        {
            Connect("pressed", this, nameof(OnHost));

            HTTPRequest reqNode = GetNode<HTTPRequest>("HTTPRequest");
            reqNode.Connect("request_completed", this, nameof(OnHostResult));
        }

        public void OnHost()
        {
            HTTPRequest reqNode = GetNode<HTTPRequest>("HTTPRequest");
            reqNode.Request("https://devnullga.me/team_factory/game", null, true, HTTPClient.Method.Post);
        }

        public void OnHostResult(int result, int response_code, string[] headers, byte[] body)
        {
            if (response_code >= 400)
            {
                GD.Print("Error getting new gameserver for hosting");
                return;
            }

            string bodyStr = Encoding.UTF8.GetString(body);
            GD.Print(bodyStr);
            JSONParseResult json = JSON.Parse(bodyStr);
            Godot.Collections.Dictionary respData = (Godot.Collections.Dictionary)json.Result;

            PackedScene lobbyPackged = GD.Load<PackedScene>("res://scenes/Lobby.tscn");
            GetNode<Node2D>("/root/Menu").QueueFree();

            LobbyNode lobbyNode = lobbyPackged.Instance<LobbyNode>();
            
            lobbyNode.ServerIP = (string)respData["ip"];
            lobbyNode.ServerPort = (int)((System.Single)respData["port"]);
            lobbyNode.LobbyCode = (string)respData["code"];

            GetTree().Root.AddChild(lobbyNode);
        }
    }
}