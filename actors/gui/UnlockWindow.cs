using Godot;
using Godot.Collections;
using TeamFactory.Game;
using TeamFactory.Util.Multiplayer;
using TeamFactory.Items;

public class UnlockWindow : WindowDialog
{
    private TextureButton sandBtn;

    private TextureButton glassBtn;

    private TextureButton siliconBtn;

    private TextureButton chipBtn;

    public override void _Ready()
    {
        ItemDB itemDB = GD.Load<ItemDB>("res://actors/items/ItemDB.tres");
        foreach(string name in itemDB.Database.Keys)
        {
            UnlockItemContainer itemContainer = GetNodeOrNull<UnlockItemContainer>($"ScrollContainer/Control/{name}Container");
            if (itemContainer != null)
                itemContainer.UnlockItem = itemDB.Database[name];
        }

        Connect("about_to_show", this, nameof(OnShow));
    }

    public void OnShow()
    {
        Dictionary<string, bool> unlocks = GetNode<GameNode>("/root/Game").PlayerUnlocks;
        ItemDB itemDB = GD.Load<ItemDB>("res://actors/items/ItemDB.tres");
        foreach(string name in itemDB.Database.Keys)
        {
            UnlockItemContainer itemContainer = GetNodeOrNull<UnlockItemContainer>($"ScrollContainer/Control/{name}Container");
            bool isUnlocked = unlocks.ContainsKey(name);
            if (itemContainer != null)
                itemContainer.IsUnlocked = isUnlocked;

            if (!isUnlocked)
            {
                bool isUnlockable = true;
                foreach(string reqItemName in itemDB.Database[name].Requirements.Keys)
                {
                    if (!unlocks.ContainsKey(reqItemName) || !unlocks[reqItemName])
                    {
                        isUnlockable = false;
                        break;
                    }
                }
                itemContainer.IsUnlockable = isUnlockable;
            }
        }
    }
}
