using Godot;
using System;
using TeamFactory.Items;
using TeamFactory.Map;

namespace TeamFactory.Factory
{
    public class FactoryWindow : WindowDialog
    {
        private TileResource tileResource;

        private OptionButton outputSelector;

        public TileResource TileResource
        {
            set {
                tileResource = value;
                UpdateWindow();
            }
        }

        public override void _Ready()
        {
            Connect("popup_hide", this, nameof(OnHide));
            outputSelector = GetNode<OptionButton>("VBoxContainer/HBoxContainer2/OptionButton");
            
            outputSelector.Connect("item_selected", this, nameof(OnSelectOutputResource));
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
            
            foreach(System.Collections.Generic.KeyValuePair<string, int> tuple in tileResource.SpawnResource.Requirements)
            {
                HBoxContainer req = reqPacked.Instance<HBoxContainer>();
                req.GetNode<Label>("Amount").Text = $"{tuple.Value}x";
                req.GetNode<TextureRect>("Input").Texture = itemDB.Database[tuple.Key].Texture;

                reqBox.AddChild(req);
            }

            GetNode<TextureRect>("VBoxContainer/Production/CenterContainer/Output").Texture = tileResource.SpawnResource.Texture;
        }
    }
}