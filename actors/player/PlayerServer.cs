using Godot;
using TeamFactory.Map;
using TeamFactory.Util.Multiplayer;

namespace TeamFactory.Player
{
    public class PlayerServer : Node
    {
        public PlayerNode Node;

        private float timeTillNextStep = 0.5f;

        private int lastSeenTargetMapIndex;

        private int[] pathToTarget;

        private Vector2[] Path;

        public override void _PhysicsProcess(float delta)
        {
            if (lastSeenTargetMapIndex != Node.TargetMapIndex)
            {
                setNewTarget(Node.TargetMapIndex);
                lastSeenTargetMapIndex = Node.TargetMapIndex;
            }

            if (Path != null && Path.Length > 0)
            {
                float totalDistance = Node.GlobalPosition.DistanceTo(Path[0]);
                if (totalDistance < 2)
                {
                    if (Path.Length > 1)
                    {
                        Path = shiftArray(Path);
                        NetState.Rpc(
                            this,
                            "SetupNextStep",
                            Node.GlobalPosition.x,
                            Node.GlobalPosition.y,
                            Path[0].x,
                            Path[0].y
                        );
                        NetState.Rpc(this, "SetupPath", Path);
                        return;
                    }
                    else
                    {
                        // end of path
                        Path = null;
                        NetState.Rpc(this, "UnsetPath");
                        return;
                    }
                }

                Node.GlobalPosition = Node.GlobalPosition.MoveToward(Path[0], 200 * delta);
            }
        }

        public void setNewTarget(int targetIndex)
        {
            MapNode mapNode = GetNode<MapNode>("../../../GridManager");

            if (Path != null && Path.Length > 0)
            {
                int lastPathIndex = mapNode.Manager.WorldToIndex(Path[0]);
                if (lastPathIndex == targetIndex)
                {
                    Path = new Vector2[] { Path[0] };
                }
                else
                {
                    Vector2 nextWorldStep = Path[0];
                    int[] indexPath = mapNode.Manager.GetPathTo(lastPathIndex, targetIndex);
                    Vector2[] endPath = mapNode.Manager.IndicesToWorld(indexPath);
                    Path = new Vector2[endPath.Length + 1];
                    System.Array.Copy(endPath, 0, Path, 1, endPath.Length);
                    Path[0] = nextWorldStep;
                }
            }
            else
            {
                int[] indexPath = mapNode.Manager.GetPathTo(mapNode.Manager.WorldToIndex(Node.GlobalPosition), targetIndex);
                Path = mapNode.Manager.IndicesToWorld(indexPath);
            }

            NetState.Rpc(this, "SetupPath", Path);            
        }

        private Vector2[] shiftArray(Vector2[] src)
        {
            if (src.Length < 1)
            {
                return src;
            }

            Vector2[] target = new Vector2[src.Length - 1];
            System.Array.Copy(src, 1, target, 0, src.Length - 1);

            return target;
        }

        [Remote]
        public void SetupNextStep(float currentX, float currentY, float targetX, float targetY)
        {
            Node.GlobalPosition = new Vector2(currentX, currentY);
            Node.NextStep = new Vector2(targetX, targetY);
        }

        [Remote]
        public void SetupPath(Vector2[] path)
        {
            Node.Path = path;
            Node.UpdatePath();
        }

        [Remote]
        public void UnsetPath()
        {
            Node.Path = null;
            Node.UpdatePath();
        }
    }
}