using Godot;
using Godot.Collections;
using System;

namespace TeamFactory.Map 
{
    public class MapResource : Resource
    {
        [Export]
        public int Width;

        [Export]
        public int Height;

        [Export]
        public int Time;

        [Export]
        public int ScoreLimit;
        
        public Vector2 SpawnPosition;

        [Export]
        public Array<string> UnlockedItems;

        [Export]
        public Array<TileResource> Tiles;

        public Array<BlockingResource> Blockings;
    }
}
