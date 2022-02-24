using Godot;
using TeamFactory.Lib.Multiplayer;

namespace TeamFactory.Items
{
    public class ItemServer : IServer
    {
        public Vector2[] Path;

        private float timeToArrive;

        private ItemNode node;

        public ItemServer(ItemNode node)
        {
            this.node = node;
        }

        public void ClientRequest(string method, params object[] args)
        {
            throw new System.NotImplementedException();
        }

        public void Tick(float delta)
        {
            if (Path != null && Path.Length > 0)
            {
                float totalDistance = node.GlobalPosition.DistanceTo(Path[0]);
                if(totalDistance < 2)
                {
                    if(Path.Length > 1)
                    {
                        Path = shiftArray(Path);
                        return;
                    }
                    else
                    {
                        // end of path
                        Path = null;
                        if (node.Target is Factory.FactoryNode targetFactory)
                        {
                            targetFactory.ItemArrived(node);
                        }
                        node.QueueFree();
                        return;
                    }
                }

                node.GlobalPosition = node.GlobalPosition.MoveToward(Path[0], 50 * delta);
                node.ServerSend("Move", node.GlobalPosition.x, node.GlobalPosition.y);
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
    }
}