using Godot;
using System.Collections.Generic;
using GodotCol = Godot.Collections;
using TeamFactory.Items;
using TeamFactory.Map;
using TeamFactory.Util.Multiplayer;
using TeamFactory.Game;
using TeamFactory.Player;

namespace TeamFactory.Infra
{
    public class InfraWindow : WindowDialog
    {
        private InfraSprite infraNode;

        private OptionButton outputSelector;

        private bool isConnecting;

        private GridManager.Direction connectionSourceDirection;
        
        private Line2D connectionLine;

        public InfraSprite InfraNode
        {
            get {
                return infraNode;
            }
            set {
                infraNode = value;
                UpdateWindow();
            }
        }

        public override void _Ready()
        {
            Connect("popup_hide", this, nameof(OnHide));

            OptionButton outputSelector = GetNode<OptionButton>("VBoxContainer/Production/ProductionChanger/OptionButton");
            outputSelector.Connect("item_selected", this, nameof(OnSelectOutputResource));
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseEvent && 
                mouseEvent.ButtonIndex == (int)ButtonList.Left && 
                mouseEvent.Pressed && 
                isConnecting
            ) {
                Vector2 pos = infraNode.GetGlobalMousePosition();
                
                MapNode mapNode = GetNode<MapNode>("/root/Game/GridManager");
                int targetIndex;
                int srcIndex;
                try
                {
                    targetIndex = mapNode.Manager.WorldToIndex(pos);
                    srcIndex = mapNode.Manager.WorldToIndex(infraNode.GlobalPosition);
                }
                catch (OutOfMapException)
                {
                    return;
                }

                if (targetIndex == srcIndex)
                    return;

                if (mapNode.IndexHasNode(targetIndex))
                {
                    isConnecting = false;
                    NetState.RpcId(mapNode, 1, "RequestConnection", srcIndex, targetIndex, connectionSourceDirection);
                    GetTree().SetInputAsHandled();
                    connectionLine.QueueFree();
                    return;
                }
                else
                {
                    GetNode<PlayerNode>($"/root/Game/Players/{NetState.NetworkId(this)}")._UnhandledInput(@event);
                }
            }
        }

        public override void _Process(float delta)
        {
            if (isConnecting && connectionLine != null)
            {
                connectionLine.RemovePoint(1);
                connectionLine.AddPoint(infraNode.GetLocalMousePosition());
            }
        }

        public void OnHide()
        {
            if (isConnecting && connectionLine != null)
            {
                connectionLine.QueueFree();
            }
            QueueFree();
        }

        public void OnSelectOutputResource(int index)
        {
            string itemName = outputSelector.GetItemText(index);
            NetState.RpcId(infraNode, 1, "RequestSpawnResourceChange", itemName);
        }

        public void UpdateWindow()
        {
            if (infraNode.SpawnResource != null)
                updateSpawnResourceData();
            
            if (infraNode.Type.Outputs.Count > 0)
                updateOutputData();

            UpdateStorage();
        }

        public void UpdateStorage()
        {
            GridContainer storage = GetNode<GridContainer>("VBoxContainer/GridContainer");
            PackedScene reqPacked = GD.Load<PackedScene>("res://actors/Infra/Requirement.tscn");
            ItemDB itemDB = GD.Load<ItemDB>("res://actors/items/ItemDB.tres");

            foreach(Control child in storage.GetChildren())
            {
                child.QueueFree();
            }

            foreach(KeyValuePair<string, int> tuple in infraNode.Storage)
            {
                HBoxContainer req = reqPacked.Instance<HBoxContainer>();
                req.GetNode<Label>("Amount").Text = $"{tuple.Value}x";
                req.GetNode<TextureRect>("Input").Texture = itemDB.Database[tuple.Key].Texture;

                storage.AddChild(req);
            }
        }

        public void OnConnectStart(GridManager.Direction dir)
        {
            connectionSourceDirection = dir;

            isConnecting = true;
            connectionLine = new Line2D();
            connectionLine.ZIndex = 90;
            connectionLine.Width = 20;
            connectionLine.TextureMode = Line2D.LineTextureMode.Tile;
            connectionLine.Material = GD.Load<ShaderMaterial>("res://materials/ConnectionLineMat.tres");
            connectionLine.AddPoint(new Vector2(0, 0));
            connectionLine.AddPoint(infraNode.GetLocalMousePosition());

            infraNode.AddChild(connectionLine);
        }

        public void OnDisconnect(GridManager.Direction dir)
        {
            MapNode mapNode = GetNode<MapNode>("/root/Game/GridManager");
            int nodeIndex = mapNode.Manager.WorldToIndex(infraNode.GlobalPosition);
            NetState.RpcId(mapNode, 1, "RequestDisconnect", nodeIndex, dir);
        }

        private void updateSpawnResourceData()
        {
            GetNode<VBoxContainer>("VBoxContainer/Production").Visible = true;

            // Production
            VBoxContainer reqBox = GetNode<VBoxContainer>("VBoxContainer/Production/CurrentProduction/Requirements");
            PackedScene reqPacked = GD.Load<PackedScene>("res://actors/Infra/Requirement.tscn");
            ItemDB itemDB = GD.Load<ItemDB>("res://actors/items/ItemDB.tres");
            
            // Requirements
            foreach(Control child in reqBox.GetChildren())
            {
                child.QueueFree();
            }
            foreach(KeyValuePair<string, int> tuple in infraNode.SpawnResource.Requirements)
            {
                HBoxContainer req = reqPacked.Instance<HBoxContainer>();
                req.GetNode<Label>("Amount").Text = $"{tuple.Value}x";

                ItemResource itemRes = itemDB.GetItemResource(tuple.Key);
                req.GetNode<TextureRect>("Input").Texture = itemRes.Texture;

                reqBox.AddChild(req);
            }

            // output resource
            GetNode<TextureRect>("VBoxContainer/Production/CurrentProduction/CenterContainer/Output").Texture = infraNode.SpawnResource.Texture;

            // Output dropdown
            outputSelector = GetNode<OptionButton>("VBoxContainer/Production/ProductionChanger/OptionButton");
            GodotCol.Dictionary<string, bool> unlockedItems = GetNode<GameNode>("/root/Game").PlayerUnlocks;
            outputSelector.Clear();

            int selected = 0;
            int i = 0;
            foreach(string option in unlockedItems.Keys)
            {
                if (!unlockedItems[option])
                    continue;
                
                if (option == infraNode.SpawnResource.Name)
                {
                    selected = i;
                }

                ItemResource item = itemDB.Database[option];
                if (item.Requirements.Count > 0 && infraNode.Type.isProducer)
                {
                    outputSelector.AddItem(option);
                    i++;
                }

                if (item.Requirements.Count <= 0 && !infraNode.Type.isProducer)
                {
                    outputSelector.AddItem(option);
                    i++;
                }
            }
            outputSelector.Selected = selected;
        }

        private void updateOutputData()
        {
            VBoxContainer connectionContainer = GetNode<VBoxContainer>("VBoxContainer/Connections");
            foreach(Node childNode in connectionContainer.GetChildren())
                childNode.QueueFree();

            PackedScene connectionPackaged = GD.Load<PackedScene>("res://actors/Infra/ConnectionButtonContainer.tscn");
            foreach (GridManager.Direction dir in infraNode.Type.GetOutputs(infraNode.Direction))
            {
                Node innerConnectionNode = connectionPackaged.Instance();
                innerConnectionNode.GetNode<Label>("Label").Text = $"{dir.ToString()} output";
                
                // Connection button setup
                Godot.Collections.Array connectionBindings = new Godot.Collections.Array();
                connectionBindings.Add(dir);
                innerConnectionNode.GetNode<Button>("ConnectBtn").Connect("pressed", this, nameof(OnConnectStart), connectionBindings);

                Button disconnectBtn = innerConnectionNode.GetNode<Button>("DisconnectBtn");
                disconnectBtn.Visible = infraNode.OutConnections.ContainsKey(dir);
                disconnectBtn.Connect("pressed", this, nameof(OnDisconnect), connectionBindings);

                connectionContainer.AddChild(innerConnectionNode);
            }
        }
    }
}