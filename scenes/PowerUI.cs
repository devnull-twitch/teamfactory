using Godot;
using System;

public class PowerUI : ColorRect
{
    private int maxValue;

    [Export]
    public int MaxValue
    {
        get
        {
            return maxValue;
        }

        set
        {
            maxValue = value;
            updateUI();
        }
    }

    private int current;

    [Export]
    public int Current
    {
        get
        {
            return current;
        }

        set
        {
            current = value;
            updateUI();
        }
    }

    private void updateUI()
    {
        if (!IsInstanceValid(this) || !IsInsideTree())
            return;

        float at = 1f - ( (float)current / ( (float)maxValue / 100f ) / 100f );
        GetNode<ColorRect>("PowerLevel").AnchorTop = at;
    }
}
