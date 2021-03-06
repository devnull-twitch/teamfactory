using Godot;
using Godot.Collections;

namespace TeamFactory.Items 
{
    public class ItemResource : Resource
    {
        [Export]
        public string Name;

        [Export]
        public int PointValue;

        [Export]
        public int UnlockCost;

        [Export]
        public int PowerValue;

        [Export]
        public int PowerCost;

        [Export]
        public Texture Texture;

        [Export]
        public Dictionary<string, int> Requirements = new Dictionary<string, int>();
    }
}