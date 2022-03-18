using Godot;
using TeamFactory.Util.Multiplayer;
using TeamFactory.Map;

namespace TeamFactory.Player 
{
    public class PlayerNode : Sprite
    {
        public int TargetMapIndex;

        public int OwnerID = 1337;

        public string PlayerName;

        public Vector2? NextStep;

        public Vector2[] Path;
    
        public override void _Ready()
        {
            PlayerServer server = new PlayerServer();
            server.Node = this;
            server.Name = "PlayerServer";
            AddChild(server);

            if (NetState.Mode == Mode.NET_SERVER)
            {
                return;
            }

            initCamera();
        }

        public override void _PhysicsProcess(float delta)
        {
            if (NextStep == null)
                return;

            float totalDistance = GlobalPosition.DistanceTo((Vector2)NextStep);
            if(totalDistance < 2)
            {
                NextStep = null;
                return;
            }

            GlobalPosition = GlobalPosition.MoveToward((Vector2)NextStep, 200 * delta);

            Line2D pathNode = GetNode<Line2D>("Node/Path");
            if (pathNode.Visible && pathNode.Points.Length > 0)
            {
                pathNode.RemovePoint(0);
                pathNode.AddPoint(GlobalPosition, 0);
            }
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

        public void UpdatePath()
        {
            Line2D pathNode = GetNode<Line2D>("Node/Path");
            if (Path == null || Path.Length <= 0)
            {
                pathNode.Visible = false;
                return;
            }

            pathNode.Visible = true;
            pathNode.ClearPoints();
            pathNode.AddPoint(GlobalPosition);
            foreach(Vector2 pv in Path)
            {
                pathNode.AddPoint(pv);
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
