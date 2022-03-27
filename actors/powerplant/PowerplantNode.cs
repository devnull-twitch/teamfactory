using Godot;
using TeamFactory.Infra;

namespace TeamFactory.Powerplant
{
    public class PowerplantNode : InfraSprite, IServerProvider
    {
        public PowerplantServer Server;
            
        public Node ServerNode {
            get {
                return Server;
            }
        }

        public override void _Ready()
        {
            Server = new PowerplantServer();
            Server.Node = this;
            Server.Name = "PowerplantServer";
            AddChild(Server);
        }
    }
}