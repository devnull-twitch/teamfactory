using Godot;
using Godot.Collections;

namespace TeamFactory.Items
{
    public class ItemDB : Resource
    {
        [Export]
        public Dictionary<string, ItemResource> Database;
    }
}
