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

        private bool isCameraFreeMode = false;

        private bool CameraFreeMode
        {
            get {
                return isCameraFreeMode;
            }
            set {
                isCameraFreeMode = value;
                TextureRect indicator = GetNode<TextureRect>("/root/Game/HUD/TopUI/HBoxContainer/CameraModeIndicator");
                if (!value)
                {
                    GetNode<Camera2D>("Camera2D").Position = new Vector2(0, 0);
                    indicator.Texture = GD.Load<Texture>("res://actors/gui/LockedCamera.png");
                }
                else
                    indicator.Texture = GD.Load<Texture>("res://actors/gui/FreeCamera.png");
            }
        }
    
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
            if (isCameraFreeMode)
            {
                if (Godot.Input.IsActionPressed("ui_left"))
                    GetNode<Camera2D>("Camera2D").MoveLocalX(-5);

                if (Godot.Input.IsActionPressed("ui_right"))
                    GetNode<Camera2D>("Camera2D").MoveLocalX(5);

                if (Godot.Input.IsActionPressed("ui_up"))
                    GetNode<Camera2D>("Camera2D").MoveLocalY(-5);

                if (Godot.Input.IsActionPressed("ui_down"))
                    GetNode<Camera2D>("Camera2D").MoveLocalY(5);
            }

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
                    CameraFreeMode = false;
                }
                catch (OutOfMapException)
                {
                    GD.Print("clicked out of map?");
                    return;
                }
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (Godot.Input.IsActionJustReleased("camera_mode_toggle"))
            {
                CameraFreeMode = !CameraFreeMode;
                GD.Print("toggle free camera");
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

        public void JumpCameraTo(Vector2 abs)
        {
            CameraFreeMode = true;
            GetNode<Camera2D>("Camera2D").GlobalPosition = abs;
        }

        public void ResetCamera()
        {
            CameraFreeMode = false;
        }
    }
}
