using Godot;
using TeamFactory.Lib.Multiplayer;
using TeamFactory.Map;

namespace TeamFactory.Player 
{
    public class PlayerNode : Sprite
    {
        public int TargetMapIndex;

        public int OwnerID = 1337;

        public string PlayerName;
    
        public override void _Ready()
        {
            PlayerServer server = new PlayerServer();
            server.Node = this;
            AddChild(server);

            if (NetState.Mode == Mode.NET_SERVER)
            {
                return;
            }

            initCamera();
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (OwnerID != NetState.NetworkId(this))
            {
                return;
            }

            if (@event is InputEventMouseButton clickEvent && clickEvent.Pressed)
            {
                try 
                {
                    MapNode mapNode = GetNode<MapNode>("../../GridManager");
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

        private void initCamera()
        {
            GD.Print("init camera called");
            if (OwnerID == NetState.NetworkId(this))
            {
                GetNode<Camera2D>("Camera2D").Current = true;
            }
        }

        [RemoteSync]
        public void RequestMoveTo(int mapIndex)
        {
            TargetMapIndex = mapIndex;
        }

        [Remote]
        public void setPosition(float x, float y)
        {
            Position = new Vector2(x, y);
        }
    }
}
