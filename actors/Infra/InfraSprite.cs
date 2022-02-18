using Godot;
using System;
using TeamFactory.Map;
using TeamFactory.Items;

namespace TeamFactory.Infra
{
    abstract public class InfraSprite : Sprite
    {
        public GridManager GridManager;

        public InfraSprite Target;

        public TileResource TileRes;

        public void RotateFromDirection(GridManager.Direction direction)
        {
            if (direction == GridManager.Direction.Up)
            {
                RotationDegrees = 90;
                return;
            }
            if (direction == GridManager.Direction.Right)
            {
                RotationDegrees = 180;
                return;
            }
            if (direction == GridManager.Direction.Down)
            {
                RotationDegrees = 270;
                return;
            }
        }

        abstract public void ItemArrived(ItemNode itemNode);
    }
}