using Godot;
using Godot.Collections;
using TeamFactory.Items;

namespace TeamFactory.Map 
{
    public class TileResource : Resource
    {
        [Export]
        public int TeamID;

        [Export]
        public int InfraPlaced;

        [Export]
        public bool IsFinal;

        [Export]
        public bool FlipX;

        [Export]
        public bool FlipY;

        [Export]
        public int ConnectedTo = -1;

        [Export]
        public float SpawnInterval;

        public Dictionary<string, int> Storage = new Dictionary<string, int>();

        [Export]
        public Dictionary<string, int> Requirements = new Dictionary<string, int>();

        public int[] PathToTarget;

        public float TimeToNextSpawn;

        [Export]
        public ItemResource SpawnResource;

        public bool IsReadyForSpawn
        {
            get {
                if (TimeToNextSpawn > 0)
                {
                    return false;
                }

                foreach(System.Collections.Generic.KeyValuePair<string, int> tuple in Requirements)
                {
                    if (!Storage.ContainsKey(tuple.Key))
                    {
                        return false;
                    }

                    if (Storage[tuple.Key] < tuple.Value)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public void PostSpawn()
        {
            if (!IsReadyForSpawn)
            {
                GD.Print("Called PostSpawn but tile isnt ready");
                return;
            }

            TimeToNextSpawn = SpawnInterval;

            foreach(System.Collections.Generic.KeyValuePair<string, int> tuple in Requirements)
            {
                Storage[tuple.Key] -= tuple.Value;
            }
        }
    }
}
