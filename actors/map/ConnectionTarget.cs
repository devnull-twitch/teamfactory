using Godot;

namespace TeamFactory.Map
{
    public class ConnectionTarget : Resource
    {
        public int MapIndex;

        public GridManager.Direction Direction;

        public ConnectionTarget(int mapIndex, GridManager.Direction direction)
        {
            this.MapIndex = mapIndex;
            this.Direction = direction;
        }
    }
}