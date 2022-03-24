using Godot;
using TeamFactory.Infra;
using Godot.Collections;
using TeamFactory.Map;
using TeamFactory.Util.Multiplayer;

namespace TeamFactory.Splitter
{
    public class SplitterNode : InfraSprite, IServerProvider, IConnectionObserver
    {
        public SplitterServer Server;

        public Array<InfraSprite> PossibleTargets;

        public Node ServerNode {
            get {
                return Server;
            }
        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Server = new SplitterServer();
            Server.Node = this;
            Server.Name = "SplitterServer";
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

        [Remote]
        public override void UpdateOutConnection(GridManager.Direction output, int x, int y, GridManager.Direction targetInputDir)
        {
            OutConnections[output] = new ConnectionTarget(new Vector2(x, y), targetInputDir);
        }

        public void NewOutConnection()
        {
            if (NetState.Mode == Mode.NET_CLIENT)
                return;
                
            Server.SetNextTarget();
        }

        public void NewInConnection()
        { }
    }
}