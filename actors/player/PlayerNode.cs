using Godot;
using TeamFactory.Lib.Multiplayer;
using TeamFactory.Map;

namespace TeamFactory.Player 
{
    public class PlayerNode : Sprite
    {
        public int TargetMapIndex;

        public int OwnerID = 1337;
    
        public override void _Ready()
        {
            PlayerServer server = new PlayerServer();
            server.Node = this;
            AddChild(server);

            if (NetState.Mode == Mode.NET_SERVER)
            {
                return;
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (OwnerID != NetState.NetworkId(this))
            {
                return;
            }

            if (@event is InputEventMouseButton clickEvent && clickEvent.Pressed)
            {
                try 
                {
                    MapNode mapNode = GetNode<MapNode>("../../");
                    int movementTargetIndex = mapNode.Manager.WorldToIndex(GetGlobalMousePosition());
                    NetState.RpcId(this, 1, "RequestMoveTo", movementTargetIndex);
                }
                catch (OutOfMapException)
                {
                    GD.Print("clicked out of map?");
                    return;
                }
            }
        }

        [RemoteSync]
        public void RequestMoveTo(int mapIndex)
        {
            TargetMapIndex = mapIndex;
        }
    }
}
