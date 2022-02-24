using Godot;
using System.Collections.Generic;
using TeamFactory.Items;
using TeamFactory.Map;

namespace TeamFactory.Factory
{
    public class FactoryWindow : WindowDialog
    {
        private TileResource tileResource;

        private FactoryNode factoryNode;

        private OptionButton outputSelector;

        private bool isConnecting;

        public FactoryNode FactoryNode
        {
            set {
                factoryNode = value;
                tileResource = value.TileRes;
                UpdateWindow();
            }
        }

        public FactoryClient FactoryClient;

        public override void _Ready()
        {
            Connect("popup_hide", this, nameof(OnHide));
            outputSelector = GetNode<OptionButton>("VBoxContainer/HBoxContainer2/OptionButton");
            GetNode<Button>("VBoxContainer/HBoxContainer2/ConnectButton").Connect("pressed", this, nameof(OnConnectStart));
            
            outputSelector.Connect("item_selected", this, nameof(OnSelectOutputResource));
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseEvent && 
                mouseEvent.ButtonIndex == (int)ButtonList.Left && 
                mouseEvent.Pressed && 
                isConnecting
            ) {
                Vector2 pos = factoryNode.GetGlobalMousePosition();
                pos = (pos / 128).Round() * 128;

                MapNode mapNode = GetNode<MapNode>("/root/Game/GridManager");
                int targetIndex = mapNode.Manager.WorldToIndex(pos);

                bool success = mapNode.Manager.ConnectTileResource(tileResource, targetIndex);
                isConnecting = false;
                GD.Print($"connection done {success}");
            }
        }

        public void OnHide()
        {
            QueueFree();
        }

        public void OnSelectOutputResource(int index)
        {
            string itemName = outputSelector.GetItemText(index);
            ItemDB itemDB = GD.Load<ItemDB>("res://actors/items/ItemDB.tres");
            tileResource.SpawnResource = itemDB.Database[itemName];
            UpdateWindow();
        }

        private void UpdateWindow()
        {
            VBoxContainer reqBox = GetNode<VBoxContainer>("VBoxContainer/Production/Requirements");
            PackedScene reqPacked = GD.Load<PackedScene>("res://actors/factory/Requirement.tscn");
            ItemDB itemDB = GD.Load<ItemDB>("res://actors/items/ItemDB.tres");
            
            foreach(Control child in reqBox.GetChildren())
            {
                child.QueueFree();
            }
            
            foreach(KeyValuePair<string, int> tuple in tileResource.SpawnResource.Requirements)
            {
                HBoxContainer req = reqPacked.Instance<HBoxContainer>();
                req.GetNode<Label>("Amount").Text = $"{tuple.Value}x";
                req.GetNode<TextureRect>("Input").Texture = itemDB.Database[tuple.Key].Texture;

                reqBox.AddChild(req);
            }

            GetNode<TextureRect>("VBoxContainer/Production/CenterContainer/Output").Texture = tileResource.SpawnResource.Texture;

            UpdateStorage();
        }

        public void UpdateStorage()
        {
            GridContainer storage = GetNode<GridContainer>("VBoxContainer/GridContainer");
            PackedScene reqPacked = GD.Load<PackedScene>("res://actors/factory/Requirement.tscn");
            ItemDB itemDB = GD.Load<ItemDB>("res://actors/items/ItemDB.tres");

            foreach(Control child in storage.GetChildren())
            {
                child.QueueFree();
            }

            foreach(KeyValuePair<string, int> tuple in FactoryClient.Storage)
            {
                HBoxContainer req = reqPacked.Instance<HBoxContainer>();
                req.GetNode<Label>("Amount").Text = $"{tuple.Value}x";
                req.GetNode<TextureRect>("Input").Texture = itemDB.Database[tuple.Key].Texture;

                storage.AddChild(req);
            }
        }

        public void OnConnectStart()
        {
            isConnecting = true;
        }
    }
}