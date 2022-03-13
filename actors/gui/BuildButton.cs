using Godot;
using System;
using TeamFactory.Map;
using TeamFactory.Lib.Multiplayer;

public class BuildButton : TextureButton
{
    [Export]
    public InfraType.TypeIdentifier InfraToBuild;

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
            MapNode mapNode = GetNode<MapNode>("/root/Game/GridManager");

            if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == (int)ButtonList.Left &&  mouseEvent.Pressed)
            {
                int mapIndex = mapNode.Manager.WorldToIndex(ghostSprite.GlobalPosition);

                GridManager.Direction dir = DirectionFromRotationDegree(ghostSprite.RotationDegrees);
                NetState.Rpc(mapNode, "RequestBuild", mapIndex, dir, InfraToBuild);
                
                inProcess = false;
                ghostSprite.QueueFree();
                return;
            }

            if (Input.IsActionPressed("rotate"))
            {
                ghostSprite.RotationDegrees += 90;
            }

            Vector2 pos = ghostSprite.GetGlobalMousePosition();
            Vector2 roundedPos = mapNode.Manager.IndexToWorld(mapNode.Manager.WorldToIndex(pos));
            ghostSprite.GlobalPosition = roundedPos;
        }
    }

    public void OnClick()
    {
        if (inProcess)
            return;
        
        InfraType it = InfraType.GetByIdentifier(InfraToBuild);

        ghostSprite = new Sprite();
        ghostSprite.Texture = it.Texture;
        ghostSprite.Material = GD.Load<ShaderMaterial>("res://materials/FactoryBtn.tres");
        GetNode<Node2D>("/root/Game").AddChild(ghostSprite);
        inProcess = true;
    }

    public GridManager.Direction DirectionFromRotationDegree(float degree)
    {
        float cappedDegree = degree % 360;
        if (cappedDegree >= 0 && cappedDegree < 90)
            return GridManager.Direction.Left;

        if (cappedDegree >= 90 && cappedDegree < 180)
            return GridManager.Direction.Up;

        if (cappedDegree >= 180 && cappedDegree < 270)
            return GridManager.Direction.Right;

        return GridManager.Direction.Down;
    }
}
