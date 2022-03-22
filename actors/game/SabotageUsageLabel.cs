using Godot;

public class SabotageUsageLabel : Label
{
    private int maxUsage = 0;

    private int currentUsage = 0;

    public int MaxUsage
    {
        set {
            maxUsage = value;
            UpdateUsages();
        }
    }

    public int CurrentUsage
    {
        set {
            currentUsage = value;
            UpdateUsages();
        }
    }

    private void UpdateUsages()
    {
        Text = $"{currentUsage} / {maxUsage}";
    }
}
