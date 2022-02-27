using Godot;
using TeamFactory.Infra;
using Godot.Collections;

namespace TeamFactory.Splitter
{
    public class SplitterNode : InfraSprite, IServerProvider
    {
        public SplitterServer Server;

        public Array<InfraSprite> PossibleTargets;

        public Node ServerNode {
            get {
                return Server;
            }
        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Server = new SplitterServer();
            Server.Node = this;
            AddChild(Server);
        }
    }
}