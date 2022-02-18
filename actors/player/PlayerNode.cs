using Godot;
using System;

namespace TeamFactory.Player 
{
    public class PlayerNode : Sprite
    {
        private PlayerClient client;

        public override void _Ready()
        {
            client = new PlayerClient(this);

            GetNode<Area2D>("Picker").Connect("input_event", this, nameof(OnInput));
        }

        public void OnInput(Node viewport, InputEvent e, int shape_idx)
        {
            if ( e is InputEventMouseButton eventMouseButton && 
                eventMouseButton.ButtonIndex == (int)ButtonList.Left &&
                eventMouseButton.Pressed == true)
            {
                client.Posess();
                return;
            }

            GetNode<Area2D>("Picker")._InputEvent(viewport, e, shape_idx);
        }
    }
}
