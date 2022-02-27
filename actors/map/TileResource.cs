using Godot;
using Godot.Collections;
using TeamFactory.Items;

namespace TeamFactory.Map 
{
    public class TileResource : Resource
    {
        [Export]
        public int TeamID;

        public int MapIndex;

        [Export]
        public PackedScene Infra;

        [Export]
        public Texture InfraTexture;

        [Export]
        public bool IsFinal;

        [Export]
        public GridManager.Direction Direction;

        [Export]
        public Dictionary<GridManager.Direction, ConnectionTarget> Connections;

        [Export]
        public float SpawnInterval;

        public Dictionary<string, int> Storage = new Dictionary<string, int>();

        public Dictionary<int, int[]> PathToTarget = new Dictionary<int, int[]>();

        [Export]
        public ItemResource SpawnResource;
    }
}
