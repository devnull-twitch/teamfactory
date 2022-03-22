using Godot;
using TeamFactory.Util.Multiplayer;
using System;

namespace TeamFactory.Game
{
    public class SabotageWindow : WindowDialog
    {
        public override void _Ready()
        {
            PackedScene packedEntry = GD.Load<PackedScene>("res://actors/game/SabotageEntry.tscn");
            VBoxContainer vBox = GetNode<VBoxContainer>("VBoxContainer");
            foreach(SabotageType sType in Enum.GetValues(typeof(SabotageType)))
            {
                Sabotage sabotageObj = Sabotage.GetSabotage(sType, null);
                
                HBoxContainer box = packedEntry.Instance<HBoxContainer>();
                box.Name = $"Sabotage_{sType}";
                box.GetNode<Label>("Label").Text = sabotageObj.Name;
                box.GetNode<SabotageUsageLabel>("Usage").MaxUsage = sabotageObj.RoundUsages;
                box.GetNode<SabotageUsageLabel>("Usage").CurrentUsage = 0;
                box.GetNode<Button>("Btn").Text = $"Buy for {sabotageObj.PointsCost}";
                Godot.Collections.Array args = new Godot.Collections.Array();
                args.Add(sType);
                box.GetNode<Button>("Btn").Connect("pressed", this, nameof(DoRequestSabotage), args);
                vBox.AddChild(box);
            }

            Connect("about_to_show", this, nameof(OnAboutToShow));
        }

        public void OnAboutToShow()
        {
            GameNode gn = GetNode<GameNode>("/root/Game");

            foreach(SabotageType sType in Enum.GetValues(typeof(SabotageType)))
            {
                int current = 0;
                gn.SabotageRoundUsages.TryGetValue(sType, out current);
                GetNode<SabotageUsageLabel>($"VBoxContainer/Sabotage_{sType}/Usage").CurrentUsage = current;
            }
        }

        public void DoRequestSabotage(SabotageType sType)
        {
            GameServer gs = GetNode<GameServer>("/root/Game/GameServer");
            NetState.RpcId(gs, 1, "RequestSabotage", sType);
            GD.Print(sType);
        }
    }
}