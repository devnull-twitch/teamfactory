using Godot;
using Godot.Collections;
using TeamFactory.Map;
using TeamFactory.Items;
using TeamFactory.Util.Multiplayer;
using TeamFactory.Player;
using TeamFactory.Game;

namespace TeamFactory.Infra
{
    abstract public class InfraSprite : Sprite
    {
        public GridManager GridManager;

        public Dictionary<string, int> Storage = new Dictionary<string, int>();

        public Dictionary<GridManager.Direction, ConnectionTarget> InConnections;

        public Dictionary<GridManager.Direction, ConnectionTarget> OutConnections;

        public int OwnerID;

        public InfraType Type;

        public ItemResource SpawnResource;

        private EnableFactoryPromt promtUi;

        public GridManager.Direction Direction;

        public float SpawnInterval;

        private bool working = false;

        public bool Working
        {
            get
            {
                return working;
            }
            set
            {
                working = value;
                if (value)
                    GetNode<Sprite>("Gear").Visible = true;
                else
                    GetNode<Sprite>("Gear").Visible = false;
            }
        }

        private bool disabled = false;

        public bool Disabled
        {
            get
            {
                return disabled;
            }
            set
            {
                disabled = value;
                GetNode<AnimatedSprite>("DisabledAnimation").Visible = disabled;
                
                if (!disabled && NetState.Mode != Mode.NET_SERVER)
                    promtUi.RemoveAvilableInfraSprite(this);
            }
        }

        [Remote]
        public void RequestDeletion()
        {
            int senderID = NetState.NetworkSenderId(this);
            if (senderID != OwnerID)
                return;

            MapNode mapNode = GetNode<MapNode>("/root/Game/GridManager");
            int selfMapIndex = mapNode.Manager.WorldToIndex(this.GlobalPosition);
            mapNode.Manager.RemoveTileResource(selfMapIndex);

            GameServer gs = GetNode<GameServer>("/root/Game/GameServer");
            gs.IncInfraToken(senderID);
        }

        public override void _Ready()
        {
            if (NetState.Mode != Mode.NET_SERVER)
            {
                promtUi = GetNode<EnableFactoryPromt>("/root/Game/HUD/CenterContainer/EnableFactoryPromt");
                GetNode<Area2D>("PlayerInteraction").Connect("area_entered", this, nameof(OnPlayerEntered));
                GetNode<Area2D>("PlayerInteraction").Connect("area_exited", this, nameof(OnPlayerLeft));
            }
        }

        public void OnPlayerEntered(Area2D playerArea)
        {
            if (playerArea.GetParent() is PlayerNode)
                promtUi.AddAvilableInfraSprite(this);
        }

        public void OnPlayerLeft(Area2D playerArea)
        {
            if (playerArea.GetParent() is PlayerNode)
                promtUi.RemoveAvilableInfraSprite(this);
        }

        public void RotateFromDirection(GridManager.Direction direction)
        {
            if (direction == GridManager.Direction.Down)
            {
                RotationDegrees = 90;
                return;
            }
            if (direction == GridManager.Direction.Left)
            {
                RotationDegrees = 180;
                return;
            }
            if (direction == GridManager.Direction.Up)
            {
                RotationDegrees = 270;
                return;
            }
        }

        [Remote]
        public virtual void UpdateOutConnection(GridManager.Direction output, int x, int y, GridManager.Direction targetInputDir)
        {
            OutConnections[output] = new ConnectionTarget(new Vector2(x, y), targetInputDir);
        }

        [Remote]
        public void ClearOutConnection(GridManager.Direction output)
        {
            OutConnections.Remove(output);
        }

        [RemoteSync]
        public void TriggereDeletion()
        {
            QueueFree();
        }

        [RemoteSync]
        public void TriggerEnable()
        {
            Disabled = false;
        }

        [RemoteSync]
        public void TriggerDisable()
        {
            Disabled = true;
        }

        [Remote]
        public void SetWorkingFlag(bool flagValue)
        {
            Working = flagValue;
        }

        public override void _Process(float delta)
        {
            if (working)
                GetNode<Sprite>("Gear").Rotation += delta;
        }
    }
}