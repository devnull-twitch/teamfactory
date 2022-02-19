using Godot;
using System;

namespace TeamFactory.Player 
{
    public class PlayerNode : Sprite
    {
        private PlayerClient client;

        private PlayerServer server;

        public override void _Ready()
        {
            server = new PlayerServer(this);
            client = new PlayerClient(this, server);

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

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseButton clickEvent && clickEvent.Pressed)
            {
                client.requestMoveTo(GetGlobalMousePosition());
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            server.Tick(delta);
        }
    }
}
