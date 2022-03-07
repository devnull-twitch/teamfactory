using Godot;
using TeamFactory.Infra;
using TeamFactory.Items;
using TeamFactory.Lib.Multiplayer;

namespace TeamFactory.Factory
{
    public class FactoryNode : InfraSprite, IServerProvider
    {
        public FactoryServer Server;

        public bool IsMulti;

        public Node ServerNode {
            get {
                return Server;
            }
        }

        public override void _Ready()
        {
            Server = new FactoryServer();
            Server.Node = this;
            AddChild(Server);

            if (IsMulti)
            {
                Texture = GD.Load<Texture>("res://actors/factory/MultiFactory.png");
            }

            if (NetState.Mode == Mode.NET_SERVER)
            {
                return;
            }

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

                PackedScene packedPanel = GD.Load<PackedScene>("res://actors/factory/FactoryWindow.tscn");
                FactoryWindow factoryWindow = packedPanel.Instance<FactoryWindow>();
                GetNode<CanvasLayer>("/root/Game/HUD").AddChild(factoryWindow);
                factoryWindow.FactoryNode = this;
                factoryWindow.Popup_();
                return;
            }

            GetNode<Area2D>("Picker")._InputEvent(viewport, e, shape_idx);
        }

        [Remote]
        public void RequestSpawnResourceChange(string itemName)
        {
            ItemDB itemDB = GD.Load<ItemDB>("res://actors/items/ItemDB.tres");
            TileRes.SpawnResource = itemDB.Database[itemName];
            NetState.Rpc(this, "SpawnResourceChange", itemName);
        }

        [RemoteSync]
        public void SpawnResourceChange(string itemName)
        {
            ItemDB itemDB = GD.Load<ItemDB>("res://actors/items/ItemDB.tres");
            TileRes.SpawnResource = itemDB.Database[itemName];
        }
    }
}