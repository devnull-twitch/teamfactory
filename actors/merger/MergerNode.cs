using Godot;
using TeamFactory.Infra;

public class MergerNode : InfraSprite, IServerProvider
{
    public MergerServer Server;

    public Node ServerNode {
        get {
            return Server;
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Server = new MergerServer();
        Server.Node = this;
        AddChild(Server);
    }
}
