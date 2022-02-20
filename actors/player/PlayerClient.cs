using Godot;
using Godot.Collections;
using TeamFactory.Map;
using TeamFactory.Gui;

namespace TeamFactory.Player 
{
    public class PlayerClient
    {
        private PlayerNode node;

        private PlayerServer server;

        private bool isPosessed = false;

        public PlayerClient(PlayerNode node, PlayerServer server)
        {
            this.node = node;
            this.server = server;
        }

        public void Posess()
        {
            node.GetNode<Camera2D>("Camera2D").MakeCurrent();
            isPosessed = true;
        }

        public void requestMoveTo(Vector2 position)
        {
            if (!isPosessed)
            {
                return;
            }

            if (!node.GetNode<Hud>("/root/Game/HUD").AllowPlayerMovement)
            {
                return;
            }
            
            MapNode mapNode = node.GetNode<MapNode>("../../");
            int movementTargetIndex = mapNode.Manager.WorldToIndex(position);
            server.setNewTarget(movementTargetIndex);
        }
    }
}