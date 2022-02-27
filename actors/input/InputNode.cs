using Godot;
using System;
using TeamFactory.Infra;
using TeamFactory.Lib.Multiplayer;

namespace TeamFactory.Input
{
    public class InputNode : InfraSprite
    {
        public override void _Ready()
        {
            if (NetState.Mode == Mode.NET_CLIENT)
            {
                return;
            }

            InputServer server = new InputServer();
            server.Node = this;
            AddChild(server);
        }
    }
}