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
        public int ConnectedTo = -1;

        [Export]
        public float SpawnInterval;

        public Dictionary<string, int> Storage = new Dictionary<string, int>();

        public int[] PathToTarget;

        [Export]
        public ItemResource SpawnResource;
    }
}
