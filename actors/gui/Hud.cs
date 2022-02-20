using Godot;
using System;

namespace TeamFactory.Gui
{
    public class Hud : CanvasLayer
    {
        public bool AllowPlayerMovement
        {
            get {
                return GetChildCount() <= 0;
            }
        }
    }
}