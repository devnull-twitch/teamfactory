using Godot;
using TeamFactory.Map;

namespace TeamFactory.Player 
{
    public class PlayerServer
    {
        private PlayerNode node;

        private int[] pathToTarget;

        private float timeTillNextStep = 0.5f;

        public PlayerServer(PlayerNode node)
        {
            this.node = node;
        }

        public void setNewTarget(int targetIndex)
        {
            MapNode mapNode = node.GetNode<MapNode>("../../");
            pathToTarget = mapNode.Manager.GetPathTo(mapNode.Manager.WorldToIndex(node.Position), targetIndex);
        }

        public void Tick(float delta)
        {
            timeTillNextStep -= delta;
            if (timeTillNextStep <= 0)
            {
                step();
                timeTillNextStep = 0.5f;
            }
        }

        private void step()
        {
            if (pathToTarget != null && pathToTarget.Length > 0)
            {
                MapNode mapNode = node.GetNode<MapNode>("../../");
                node.Position = mapNode.Manager.IndexToWorld(pathToTarget[0]);

                if (pathToTarget.Length <= 1)
                {
                    pathToTarget = null;
                    return;
                }

                int[] newPath = new int[pathToTarget.Length - 1];
                System.Array.Copy(pathToTarget, 1, newPath, 0, newPath.Length);
                pathToTarget = newPath;
            }
        }
    }
}