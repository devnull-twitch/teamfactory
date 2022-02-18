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
        public Array<TileResource> Tiles;
    }
}
