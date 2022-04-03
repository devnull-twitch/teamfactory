using Godot;
using Godot.Collections;
using TeamFactory.Game;
using TeamFactory.Items;

public class UnlockWindow : WindowDialog
{
    private Dictionary<string, UnlockItemContainer> itemContainers = new Dictionary<string, UnlockItemContainer>();

    private bool linesInit = false;

    public override void _Ready()
    {
        Connect("about_to_show", this, nameof(OnShow));

        ItemDB itemDB = GD.Load<ItemDB>("res://actors/items/ItemDB.tres");
        Control scrollContent = GetNode<Control>("ScrollContainer/Control");
        foreach (string name in itemDB.Database.Keys)
        {
            int row = 1;
            bool continueRowSearch = true;
            while (continueRowSearch)
            {
                Control rowNode = scrollContent.GetNodeOrNull<Control>($"VBoxContainer/Columns/Row{row}");
                if (rowNode == null)
                {
                    continueRowSearch= false;
                    continue;
                }

                UnlockItemContainer itemContainer = scrollContent.GetNodeOrNull<UnlockItemContainer>($"VBoxContainer/Columns/Row{row}/InnerRow/{name}Container");
                if (itemContainer == null)
                {
                    row++;
                    continue;
                }

                continueRowSearch = false;
                itemContainers[name] = itemContainer;
            }
        }

        foreach (string name in itemDB.Database.Keys)
        {
            if (!itemContainers.ContainsKey(name))
                continue;

            ItemResource item = itemDB.Database[name];
            UnlockItemContainer itemContainer = itemContainers[name];
            itemContainer.UnlockItem = item;
        }
    }

    public override void _Draw()
    {
        base._Draw();

        if (Visible && !linesInit)
            drawLines();
    }

    public void OnShow()
    {
        Dictionary<string, bool> unlocks = GetNode<GameNode>("/root/Game").PlayerUnlocks;
        ItemDB itemDB = GD.Load<ItemDB>("res://actors/items/ItemDB.tres");
        foreach (string name in itemDB.Database.Keys)
        {
            if (!itemContainers.ContainsKey(name))
                continue;

            UnlockItemContainer itemContainer = itemContainers[name];
            bool isUnlocked = unlocks.ContainsKey(name);
            itemContainer.IsUnlocked = isUnlocked;

            if (!isUnlocked)
            {
                bool isUnlockable = true;
                foreach (string reqItemName in itemDB.Database[name].Requirements.Keys)
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

    private void drawLines()
    {
        linesInit = true;
        ItemDB itemDB = GD.Load<ItemDB>("res://actors/items/ItemDB.tres");
        foreach (string name in itemDB.Database.Keys)
        {
            if (!itemContainers.ContainsKey(name))
                continue;

            ItemResource item = itemDB.Database[name];
            UnlockItemContainer itemContainer = itemContainers[name];
            if (item.Requirements.Count > 0)
            {
                foreach (string reqItemName in item.Requirements.Keys)
                {
                    if (!itemContainers.ContainsKey(reqItemName))
                        continue;

                    UnlockItemContainer lineTargetContainer = itemContainers[reqItemName];
                    if (lineTargetContainer != null)
                    {
                        MarginContainer marginCnt = itemContainer.GetNode<MarginContainer>("../../");
                        float margin = (float)marginCnt.GetConstant("margin_right");
                        Vector2 diff = lineTargetContainer.RectGlobalPosition - itemContainer.RectGlobalPosition;
                        Vector2 start = itemContainer.RectPosition + new Vector2(margin, 52);
                        Vector2 end = itemContainer.RectPosition + diff + new Vector2(margin + lineTargetContainer.RectSize.x, 52);
                        Line2D connectionLine = new Line2D();
                        connectionLine.AddPoint(start);
                        connectionLine.AddPoint(end);
                        connectionLine.Width = 2;
                        
                        marginCnt.AddChild(connectionLine);
                    }
                }
            }
        }
    }
}
