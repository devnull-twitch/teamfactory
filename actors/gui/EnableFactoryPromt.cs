using Godot;
using Godot.Collections;
using TeamFactory.Infra;
using TeamFactory.Util.Multiplayer;
using TeamFactory.Game;
using TeamFactory.Map;

public class EnableFactoryPromt : ColorRect
{
    private Array<InfraSprite> availableInfraSprites = new Array<InfraSprite>();

    public void AddAvilableInfraSprite(InfraSprite infraNode)
    {
        if (availableInfraSprites.Contains(infraNode))
            return;

        if (!infraNode.Disabled)
            return;

        availableInfraSprites.Add(infraNode);
        updateUI();
    }

    public void RemoveAvilableInfraSprite(InfraSprite infraNode)
    {
        availableInfraSprites.Remove(infraNode);
        updateUI();
    }

    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionJustPressed("enable_infra") && availableInfraSprites.Count > 0)
        {
            InfraSprite infraNode = availableInfraSprites[0];
            availableInfraSprites.RemoveAt(0);

            GameServer gs = GetNode<GameServer>("/root/Game/GameServer");
            MapNode mapNode = GetNode<MapNode>("/root/Game/GridManager");
            NetState.RpcId(gs, 1, "RequestEnable", mapNode.Manager.WorldToIndex(infraNode.GlobalPosition));
        }
    }

    private void updateUI()
    {
        Visible = availableInfraSprites.Count > 0;
    }
}
