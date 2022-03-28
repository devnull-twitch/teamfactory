using Godot;
using TeamFactory.Infra;
using TeamFactory.Items;
using TeamFactory.Util.Multiplayer;

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
            Server.Name = "FactoryServer";
            AddChild(Server);

            base._Ready();

            if (IsMulti)
                Texture = GD.Load<Texture>("res://actors/factory/MultiFactory.png");

            if (NetState.Mode == Mode.NET_SERVER)
                return;

            GetNode<Area2D>("Picker").Connect("input_event", this, nameof(OnInput));
        }

        public void OnInput(Node viewport, InputEvent e, int shape_idx)
        {
            if ( e is InputEventMouseButton eventMouseButton && 
                eventMouseButton.ButtonIndex == (int)ButtonList.Left &&
                eventMouseButton.Pressed == true)
            {
                if (GetNodeOrNull<CanvasLayer>("/root/Game/HUD/FactoryPanel") != null)
                    return;

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
        public void RequestSpawnResourceChange(string itemName)
        {
            ItemDB itemDB = GD.Load<ItemDB>("res://actors/items/ItemDB.tres");
            SpawnResource = itemDB.Database[itemName];
            NetState.Rpc(this, "SpawnResourceChange", itemName);
        }

        [Remote]
        public void SpawnResourceChange(string itemName)
        {
            ItemDB itemDB = GD.Load<ItemDB>("res://actors/items/ItemDB.tres");
            SpawnResource = itemDB.Database[itemName];

            InfraWindow window = GetNodeOrNull<InfraWindow>("/root/Game/HUD/Panel");
            if (window != null && window.InfraNode == this)
                window.UpdateWindow();
        }
    }
}