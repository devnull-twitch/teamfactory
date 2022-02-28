using Godot;
using System;
using TeamFactory.Map;

public class BuildButton : TextureButton
{
    [Export]
    public TileResource InfraToBuild;

    private bool inProcess;

    private Sprite ghostSprite;

    public override void _Ready()
    {
        Connect("pressed", this, nameof(OnClick));
    }

    public override void _Input(InputEvent @event)
    {
        if (inProcess)
        {
            if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == (int)ButtonList.Left &&  mouseEvent.Pressed)
            {
                MapNode mapNode = GetNode<MapNode>("/root/Game/GridManager");
                int mapIndex = mapNode.Manager.WorldToIndex(ghostSprite.GlobalPosition);

                TileResource newTR = new TileResource();
                newTR.Infra = InfraToBuild.Infra;
                // TODO get current player team ID from NetState
                newTR.OwnerID = 1337;
                newTR.SpawnResource = InfraToBuild.SpawnResource;
                newTR.SpawnInterval = 2;
                mapNode.Manager.AddTileResouce(newTR, mapIndex);
                inProcess = false;
                ghostSprite.QueueFree();
                return;
            }

            if (Input.IsActionPressed("rotate"))
            {
                ghostSprite.RotationDegrees += 90;
            }

            Vector2 pos = ghostSprite.GetGlobalMousePosition();
            ghostSprite.GlobalPosition = (pos / 128).Round() * 128 + (new Vector2(-64, -64));
        }
    }

    public void OnClick()
    {
        if (inProcess)
        {
            return;
        }

        ghostSprite = new Sprite();
        ghostSprite.Texture = InfraToBuild.InfraTexture;
        ghostSprite.Material = GD.Load<ShaderMaterial>("res://materials/FactoryBtn.tres");
        GetNode<Node2D>("/root/Game").AddChild(ghostSprite);
        inProcess = true;
    }
}
