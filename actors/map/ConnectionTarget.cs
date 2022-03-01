using Godot;

namespace TeamFactory.Map
{
    public class ConnectionTarget : Resource
    {
        public Vector2 TargetCoords;

        public GridManager.Direction Direction;

        public ConnectionTarget(Vector2 targetCoords, GridManager.Direction direction)
        {
            this.TargetCoords = targetCoords;
            this.Direction = direction;
        }
    }
}