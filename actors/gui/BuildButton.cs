using Godot;
using System;
using TeamFactory.Map;

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
            if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == (int)ButtonList.Left &&  mouseEvent.Pressed)
            {
                MapNode mapNode = GetNode<MapNode>("/root/Game/GridManager");
                int mapIndex = mapNode.Manager.WorldToIndex(ghostSprite.GlobalPosition);

                // TODO make rpc call to mapnode server to spawn new InfraSprite node
                
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
            return;
        
        InfraType it = InfraType.GetByIdentifier(InfraToBuild);

        ghostSprite = new Sprite();
        ghostSprite.Texture = it.Texture;
        ghostSprite.Material = GD.Load<ShaderMaterial>("res://materials/FactoryBtn.tres");
        GetNode<Node2D>("/root/Game").AddChild(ghostSprite);
        inProcess = true;
    }
}
