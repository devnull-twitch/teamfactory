using Godot;
using System;
using TeamFactory.Map;
using TeamFactory.Util.Multiplayer;

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

            try {
                Vector2 pos = ghostSprite.GetGlobalMousePosition();
                Vector2 roundedPos = mapNode.Manager.IndexToWorld(mapNode.Manager.WorldToIndex(pos));
                ghostSprite.GlobalPosition = roundedPos;
            } 
            catch (OutOfMapException) { }
        }
    }

    public void OnClick()
    {
        if (inProcess)
            return;
        
        InfraType it = InfraType.GetByIdentifier(InfraToBuild);
        ghostSprite = new Sprite();
        ghostSprite.Texture = it.Texture;
        ShaderMaterial shaderMat = GD.Load<ShaderMaterial>("res://materials/FactoryBtn.tres");
        shaderMat.SetShaderParam("ButtonTexture", it.Texture);
        ghostSprite.Material = shaderMat;
        GetNode<Node2D>("/root/Game").AddChild(ghostSprite);
        inProcess = true;
    }

    public GridManager.Direction DirectionFromRotationDegree(float degree)
    {
        float cappedDegree = degree % 360;
        if (cappedDegree >= 0 && cappedDegree < 90)
            return GridManager.Direction.Right;

        if (cappedDegree >= 90 && cappedDegree < 180)
            return GridManager.Direction.Down;

        if (cappedDegree >= 180 && cappedDegree < 270)
            return GridManager.Direction.Left;

        return GridManager.Direction.Up;
    }
}
