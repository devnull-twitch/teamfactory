using Godot;
using TeamFactory.Infra;

namespace TeamFactory.Merger
{
    public class MergerNode : InfraSprite, IServerProvider
    {
        public MergerServer Server;

        public Node ServerNode {
            get {
                return Server;
            }
        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Server = new MergerServer();
            Server.Node = this;
            Server.Name = "MergerServer";
            AddChild(Server);

            GetNode<Area2D>("Picker").Connect("input_event", this, nameof(OnInput));
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