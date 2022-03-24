using Godot;
using Godot.Collections;
using TeamFactory.Map;
using TeamFactory.Items;

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

        public float SpawnInterval;

        public void RotateFromDirection(GridManager.Direction direction)
        {
            if (direction == GridManager.Direction.Up)
            {
                RotationDegrees = 90;
                return;
            }
            if (direction == GridManager.Direction.Right)
            {
                RotationDegrees = 180;
                return;
            }
            if (direction == GridManager.Direction.Down)
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
    }
}