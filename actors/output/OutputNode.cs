using Godot;
using System;
using TeamFactory.Infra;
using TeamFactory.Lib.Multiplayer;

namespace TeamFactory.Output
{
    public class OutputNode : InfraSprite, IServerProvider
    {
        public OutputServer Server;
        
        public Node ServerNode {
            get {
                return Server;
            }
        }

        public override void _Ready()
        {
            Server = new OutputServer();
            Server.Node = this;
            Server.Name = "OutputServer";
            AddChild(Server);
        }
    }
}