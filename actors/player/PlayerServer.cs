using Godot;
using TeamFactory.Map;

namespace TeamFactory.Player 
{
    public class PlayerServer : Node
    {
        public PlayerNode Node;

        private float timeTillNextStep = 0.5f;

        private int lastSeenTargetMapIndex;

        private int[] pathToTarget;

        public override void _PhysicsProcess(float delta)
        {
            if (lastSeenTargetMapIndex != Node.TargetMapIndex)
            {
                setNewTarget(Node.TargetMapIndex);
                lastSeenTargetMapIndex = Node.TargetMapIndex;
            }

            timeTillNextStep -= delta;
            if (timeTillNextStep <= 0)
            {
                step();
                timeTillNextStep = 0.5f;
            }
        }

        public void setNewTarget(int targetIndex)
        {
            MapNode mapNode = GetNode<MapNode>("../../../");
            pathToTarget = mapNode.Manager.GetPathTo(mapNode.Manager.WorldToIndex(Node.GlobalPosition), targetIndex);
        }

        private void step()
        {
            if (pathToTarget != null && pathToTarget.Length > 0)
            {
                MapNode mapNode = GetNode<MapNode>("../../../");
                Node.Position = mapNode.Manager.IndexToWorld(pathToTarget[0]);

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