using Godot;
using System;
using TeamFactory.Lib.Multiplayer;
using TeamFactory.Map;

namespace TeamFactory.Player 
{
    public class PlayerNode : Sprite
    {
        public int PosessedBy;

        public int TargetMapIndex;
    
        public override void _Ready()
        {
            PlayerServer server = new PlayerServer();
            server.Node = this;
            AddChild(server);

            if (NetState.Mode == Mode.NET_SERVER)
            {
                return;
            }

            GetNode<Area2D>("Picker").Connect("input_event", this, nameof(OnInput));
        }

        public void OnInput(Node viewport, InputEvent e, int shape_idx)
        {
            if ( e is InputEventMouseButton eventMouseButton && 
                eventMouseButton.ButtonIndex == (int)ButtonList.Left &&
                eventMouseButton.Pressed == true)
            {
                NetState.RpcId(this, 1, "RequestPosession");
                return;
            }

            GetNode<Area2D>("Picker")._InputEvent(viewport, e, shape_idx);
        }

        [RemoteSync]
        public void RequestPosession()
        {
            if (PosessedBy != 0)
            {
                return;
            }

            PosessedBy = NetState.NetworkSenderId(this);
            NetState.Rpc(this, "Posess", PosessedBy);
        }

        [RemoteSync]
        public void Posess(int playerNetworkID)
        {
            PosessedBy = playerNetworkID;

            if (PosessedBy == NetState.NetworkId(this))
            {
                GetNode<Camera2D>("Camera2D").MakeCurrent();
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (PosessedBy != NetState.NetworkId(this))
            {
                return;
            }

            if (@event is InputEventMouseButton clickEvent && clickEvent.Pressed)
            {
                MapNode mapNode = GetNode<MapNode>("../../");
                int movementTargetIndex = mapNode.Manager.WorldToIndex(GetGlobalMousePosition());
                NetState.RpcId(this, 1, "RequestMoveTo", movementTargetIndex);
            }
        }

        [RemoteSync]
        public void RequestMoveTo(int mapIndex)
        {
            TargetMapIndex = mapIndex;
        }
    }
}
