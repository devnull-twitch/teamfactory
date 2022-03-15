using Godot;
using System;
using TeamFactory.Infra;
using TeamFactory.Util.Multiplayer;

namespace TeamFactory.Input
{
    public class InputNode : InfraSprite
    {
        private InputServer server;

        public override void _Ready()
        {
            GetNode<Area2D>("Picker").Connect("input_event", this, nameof(OnInput));

            server = new InputServer();
            server.Node = this;
            server.Name = "InputServer";
            AddChild(server);
        }

        public void OnInput(Node viewport, InputEvent e, int shape_idx)
        {
            if ( e is InputEventMouseButton eventMouseButton && 
                eventMouseButton.ButtonIndex == (int)ButtonList.Left &&
                eventMouseButton.Pressed == true)
            {
                if (GetNodeOrNull<CanvasLayer>("/root/Game/HUD/FactoryPanel") != null)
                {
                    return;
                }

                PackedScene packedPanel = GD.Load<PackedScene>("res://actors/Infra/InfraWindow.tscn");
                InfraWindow infraWindow = packedPanel.Instance<InfraWindow>();
                GetNode<CanvasLayer>("/root/Game/HUD").AddChild(infraWindow);
                infraWindow.InfraNode = this;
                infraWindow.Popup_();
                return;
            }

            GetNode<Area2D>("Picker")._InputEvent(viewport, e, shape_idx);
        }
    }
}