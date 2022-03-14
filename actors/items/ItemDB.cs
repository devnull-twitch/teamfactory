using Godot;
using Godot.Collections;

namespace TeamFactory.Items
{
    public class ItemDB : Resource
    {
        [Export]
        public Dictionary<string, ItemResource> Database;

        public ItemResource GetItemResource(string name)
        {
            if (!Database.ContainsKey(name))
                throw new System.Exception($"missing item {name} in ItemDB");

            return Database[name];
        }
    }
}
