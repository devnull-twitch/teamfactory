using Godot;
using System;

namespace TeamFactory.Items 
{
    public class ItemResource : Resource
    {
        [Export]
        public string Name;

        [Export]
        public int PointValue;

        [Export]
        public Texture Texture;
    }
}