using Godot;
using Godot.Collections;
using TeamFactory.Items;

namespace TeamFactory.Map 
{
    public class TileResource : Resource
    {
        public Vector2 Coords;

        public int OwnerID;

        public InfraType.TypeIdentifier InfraTypeIdentifier;

        public Dictionary<string, string> InfraOptions;

        public bool IsFinal;

        public bool IsLocked;
        
        public GridManager.Direction Direction;

        public Dictionary<GridManager.Direction, ConnectionTarget> Connections;

        public ItemResource SpawnResource;
    }
}
