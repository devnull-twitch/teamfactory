using Godot;
using System;

namespace TeamFactory.Items 
{
    public class ItemNode : Sprite
    {
        private ItemResource item;

        public Vector2[] Path;

        public Map.ArrivalCallback Callback;

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

        public override void _Ready()
        {
            
        }

        public override void _PhysicsProcess(float delta)
        {
            if (Path != null && Path.Length > 0)
            {
                float totalDistance = Position.DistanceTo(Path[0]);
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
                        Callback.ItemArrived(Item);
                        QueueFree();
                        return;
                    }
                }

                Position = Position.MoveToward(Path[0], 50 * delta);
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