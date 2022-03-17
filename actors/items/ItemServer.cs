using Godot;
using Godot.Collections;
using TeamFactory.Map;
using TeamFactory.Util.Multiplayer;

namespace TeamFactory.Items
{
    public class ItemServer : Node
    {
        private float timeToArrive;

        public ItemNode Node;

        private Vector2[] Path;

        public override void _Ready()
        {
            if (NetState.Mode == Mode.NET_CLIENT)
            {
                return;
            }

            GridManager gm = GetNode<MapNode>("/root/Game/GridManager").Manager;
            int fromIndex = gm.WorldToIndex(Node.GlobalPosition); 
            int targetIndex = gm.WorldToIndex(Node.Target.GlobalPosition);
            int[] indexPath = gm.GetConnectionPath(fromIndex, targetIndex);
            Path = gm.IndicesToWorld(indexPath);
        }

        public override void _PhysicsProcess(float delta)
        {
            // TODO: check if node has target but Path is null and then calc path
            if (Path != null && Path.Length > 0)
            {
                float totalDistance = Node.GlobalPosition.DistanceTo(Path[0]);
                if(totalDistance < 2)
                {
                    if(Path.Length > 1)
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
                        return;
                    }
                    else
                    {
                        // end of path
                        Path = null;
                        if (Node.Target is Infra.IServerProvider serverProviderNode)
                        {
                            Node serverNode = serverProviderNode.ServerNode;
                            if (serverNode is Infra.IItemReceiver itemReceiverNode)
                            {
                                itemReceiverNode.ItemArrived(Node);
                            }
                        }
                        NetState.Rpc(this, "Delete");
                        return;
                    }
                }

                Node.GlobalPosition = Node.GlobalPosition.MoveToward(Path[0], 200 * delta);
            }
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

        [RemoteSync]
        public void Move(float x, float y)
        {
            Node.GlobalPosition = new Vector2(x, y);
        }

        [Remote]
        public void SetupNextStep(float currentX, float currentY, float targetX, float targetY)
        {
            Node.GlobalPosition = new Vector2(currentX, currentY);
            Node.NextStep = new Vector2(targetX, targetY);
        } 

        [RemoteSync]
        public void Delete()
        {
            Node.QueueFree();
        }
    }
}