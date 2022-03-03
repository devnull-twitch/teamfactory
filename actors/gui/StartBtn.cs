using Godot;
using TeamFactory.Lib.Multiplayer;

namespace TeamFactory.Gui
{
    public class StartBtn : Button
    {
        public override void _Ready()
        {
            Connect("pressed", this, nameof(OnReady));
        }

        public void OnReady()
        {
            LobbyServer lobbyServerNode = GetNode<LobbyServer>("/root/Lobby/LobbyServer");
            NetState.RpcId(lobbyServerNode, 1, "PlayerRequestStart");
        }
    }
}