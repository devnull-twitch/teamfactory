using Godot;

namespace TeamFactory.Player 
{
    public class PlayerClient
    {
        private PlayerNode node;

        public PlayerClient(PlayerNode node)
        {
            this.node = node;
        }

        public void Posess()
        {
            node.GetNode<Camera2D>("Camera2D").MakeCurrent();
        }
    }
}