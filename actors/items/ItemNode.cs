using Godot;
using System;
using TeamFactory.Lib.Multiplayer;
using TeamFactory.Map;

namespace TeamFactory.Items 
{
    public class ItemNode : Sprite
    {
        private ItemResource item;

        public Infra.InfraSprite Target;

        public Vector2[] Path;

        private NodeWrapper<ItemNode, ItemServer, ItemClient> multiplayerWrapper;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            ItemServer server = new ItemServer(this);
            ItemClient client = new ItemClient(this);
            multiplayerWrapper = new NodeWrapper<ItemNode, ItemServer, ItemClient>(this, server, client);

            ZIndex = ZIndex + 5;
        }

        public ItemResource Item
        {
            set{
                item = value;
                Texture = value.Texture;
            }
            get {
                return item;
            }
        }

        public override void _Notification(int what)
        {
            if (multiplayerWrapper != null)
            {
                multiplayerWrapper.Notification(what);
            }

            base._Notification(what);
        }

        public override void _PhysicsProcess(float delta)
        {
            if (Path != null && Path.Length > 0)
            {
                float totalDistance = GlobalPosition.DistanceTo(Path[0]);
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
                        if (Target is Factory.FactoryNode targetFactory)
                        {
                            targetFactory.ItemArrived(this);
                        }
                        QueueFree();
                        return;
                    }
                }

                GlobalPosition = GlobalPosition.MoveToward(Path[0], 50 * delta);
            }
        }

        private Vector2[] shiftArray(Vector2[] src)
        {
            if (src.Length < 1)
            {
                return src;
            }

            Vector2[] target = new Vector2[src.Length - 1];
            Array.Copy(src, 1, target, 0, src.Length - 1);

            return target;
        }
    }
}