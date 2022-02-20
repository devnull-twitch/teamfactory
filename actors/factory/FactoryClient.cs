using Godot;
using TeamFactory.Lib.Multiplayer;
using TeamFactory.Items;

namespace TeamFactory.Factory
{
    public class FactoryClient : Reference, IClient
    {
        private FactoryNode node;

        public FactoryClient(FactoryNode node)
        {
            this.node = node;

            node.GetNode<Area2D>("Picker").Connect("input_event", this, nameof(OnInput));
        }

        public void OnInput(Node viewport, InputEvent e, int shape_idx)
        {
            if ( e is InputEventMouseButton eventMouseButton && 
                eventMouseButton.ButtonIndex == (int)ButtonList.Left &&
                eventMouseButton.Pressed == true)
            {
                if (node.GetNodeOrNull<CanvasLayer>("FactoryPanel") != null)
                {
                    return;
                }

                PackedScene packedPanel = GD.Load<PackedScene>("res://actors/factory/FactoryWindow.tscn");
                FactoryWindow factoryWindow = packedPanel.Instance<FactoryWindow>();
                node.GetNode<CanvasLayer>("/root/Game/HUD").AddChild(factoryWindow);
                factoryWindow.TileResource = node.TileRes;
                factoryWindow.Popup_();
                return;
            }

            node.GetNode<Area2D>("Picker")._InputEvent(viewport, e, shape_idx);
        }

        public void ServerRequest(string method, params object[] args)
        {
            switch (method)
            {
                case "Destroy":
                    node.QueueFree();
                    break;

                case "SpawnItem":
                    SpawnItem();
                    break;

                default:
                    GD.PrintErr("Unknown request to FactoryClient", method);
                    break;
            }
        }

        private void SpawnItem()
        {
            if (NetState.Mode == Mode.LOCAL)
            {
                return;
            }
            node.SpawnItem();
        }
    }
}