using Godot;
using TeamFactory.Util.Multiplayer;

namespace TeamFactory.Game
{
    public class SabotageWindow : WindowDialog
    {
        public override void _Ready()
        {
            GetNode<Button>("GridContainer/DeleteConnectionBtn").Connect("pressed", this, nameof(DoDeleteConnection));
            GetNode<Button>("GridContainer/TurnFactoryBlockBtn").Connect("pressed", this, nameof(DoTurnFactoryToBlockHole));

            Connect("about_to_show", this, nameof(OnAboutToShow));
        }

        public void OnAboutToShow()
        {
            GameNode gn = GetNode<GameNode>("/root/Game");

            int deleteCount = 0;
            gn.SabotageRoundUsages.TryGetValue(SabotageType.DeleteConnection, out deleteCount);
            SabotageUsageLabel disconnectSUL = GetNode<SabotageUsageLabel>("GridContainer/DeleteConnectionUsage");
            disconnectSUL.CurrentUsage = deleteCount;
            disconnectSUL.MaxUsage = 3;

            int turnToBlockCount = 0;
            gn.SabotageRoundUsages.TryGetValue(SabotageType.TurnFactoryToBlock, out turnToBlockCount);
            SabotageUsageLabel turnToBlockSUL = GetNode<SabotageUsageLabel>("GridContainer/TurnFactoryBlockUsage");
            turnToBlockSUL.CurrentUsage = turnToBlockCount;
            turnToBlockSUL.MaxUsage = 1;
        }

        public void DoDeleteConnection()
        {
            GameServer gs = GetNode<GameServer>("/root/Game/GameServer");
            NetState.RpcId(gs, 1, "RequestSabotage", SabotageType.DeleteConnection);
            GD.Print(SabotageType.DeleteConnection);
        }

        public void DoTurnFactoryToBlockHole()
        {
            GameServer gs = GetNode<GameServer>("/root/Game/GameServer");
            NetState.RpcId(gs, 1, "RequestSabotage", SabotageType.TurnFactoryToBlock);
            GD.Print(SabotageType.TurnFactoryToBlock);
        }
    }
}