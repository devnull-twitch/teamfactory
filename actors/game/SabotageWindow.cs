using Godot;
using TeamFactory.Lib.Multiplayer;

namespace TeamFactory.Game
{
    public class SabotageWindow : WindowDialog
    {
        public override void _Ready()
        {
            GetNode<Button>("GridContainer/DeleteConnectionBtn").Connect("pressed", this, nameof(DoDeleteConnection));
        }

        public void DoDeleteConnection()
        {
            GameServer gs = GetNode<GameServer>("/root/Game/GameServer");
            NetState.RpcId(gs, 1, "RequestSabotage", SabotageType.DeleteConnection);
            GD.Print(SabotageType.DeleteConnection);
        }
    }
}