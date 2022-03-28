using Godot;
using System;

namespace TeamFactory.Powerplant
{
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

                if (!IsInstanceValid(this) || !IsInsideTree())
                    return;
                
                GetNode<Label>("Max").Text = $"{value}";
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

        public override void _Ready()
        {
            GetNode<Label>("Max").Text = $"{maxValue}";
            GetNode<Label>("Current").Text = $"0";
        }

        private void updateUI()
        {
            if (!IsInstanceValid(this) || !IsInsideTree())
                return;

            float at = 1f - ((float)current / ((float)maxValue / 100f) / 100f);
            GetNode<ColorRect>("PowerLevel").AnchorTop = at;
            GetNode<Label>("Current").Text = $"{current}";
        }
    }
}