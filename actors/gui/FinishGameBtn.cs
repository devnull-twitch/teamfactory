using Godot;
using System;

namespace TeamFactory.Gui
{
    public class FinishGameBtn : Button
    {
        public override void _Ready()
        {
            Connect("pressed", this, "OnPress");
        }

        public void OnPress()
        {
            GetTree().Quit();
        }
    }
}