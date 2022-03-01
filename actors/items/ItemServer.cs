using Godot;
using TeamFactory.Lib.Multiplayer;

namespace TeamFactory.Items
{
    public class ItemServer : Node
    {
        private float timeToArrive;

        public ItemNode Node;

        public override void _PhysicsProcess(float delta)
        {
            if (Node.Path != null && Node.Path.Length > 0)
            {
                float totalDistance = Node.GlobalPosition.DistanceTo(Node.Path[0]);
                if(totalDistance < 2)
                {
                    if(Node.Path.Length > 1)
                    {
                        Node.Path = shiftArray(Node.Path);
                        return;
                    }
                    else
                    {
                        // end of path
                        Node.Path = null;
                        if (Node.Target is Infra.IServerProvider serverProviderNode)
                        {
                            Node serverNode = serverProviderNode.ServerNode;
                            if (serverNode is Infra.IItemReceiver itemReceiverNode)
                            {
                                itemReceiverNode.ItemArrived(Node);
                            }
                        }
                        Node.QueueFree();
                        return;
                    }
                }

                Vector2 newPosition = Node.GlobalPosition.MoveToward(Node.Path[0], 200 * delta);
                NetState.Rpc(this, "Move", newPosition.x, newPosition.y);
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
    }
}