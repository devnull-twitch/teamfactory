using Godot;
using Godot.Collections;
using TeamFactory.Lib.Multiplayer;
using TeamFactory.Items;

namespace TeamFactory.Factory
{
    public class FactoryClient : Reference, IClient
    {
        private FactoryNode node;

        private FactoryWindow factoryWindow;

        public Dictionary<string, int> Storage = new Dictionary<string, int>();

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
                factoryWindow = packedPanel.Instance<FactoryWindow>();
                node.GetNode<CanvasLayer>("/root/Game/HUD").AddChild(factoryWindow);
                factoryWindow.FactoryClient = this;
                factoryWindow.FactoryNode = node;
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

                case "StorageUpdate":
                    string keyName = (string)args[0];
                    int storageVal = (int)args[1];
                    Storage[keyName] = storageVal;
                    if (FactoryWindow.IsInstanceValid(factoryWindow))
                    {
                        GD.Print($"{keyName}={storageVal}");
                        factoryWindow.UpdateStorage();
                    }
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