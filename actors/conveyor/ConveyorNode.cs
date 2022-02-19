using Godot;
using System;
using TeamFactory.Map;

namespace TeamFactory.Conveyor
{
    public class ConveyorNode : Sprite
    {
        

        [Export]
        public Texture Straight;

        [Export]
        public Texture Curve1;

        [Export]
        public Texture Curve2;

        [Export]
        public GridManager.Direction InputDir;

        [Export]
        public GridManager.Direction OutputDir;


        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            if (InputDir == GridManager.Direction.Left && OutputDir == GridManager.Direction.Right)
            {
                Texture = Straight;
            }
            if (InputDir == GridManager.Direction.Right && OutputDir == GridManager.Direction.Left)
            {
                Texture = Straight;
                RotationDegrees = 180;
            }
            if (InputDir == GridManager.Direction.Up && OutputDir == GridManager.Direction.Down)
            {
                Texture = Straight;
                RotationDegrees = 90;
            }
            if (InputDir == GridManager.Direction.Down && OutputDir == GridManager.Direction.Up)
            {
                Texture = Straight;
                RotationDegrees = 270;
            }

            if (InputDir == GridManager.Direction.Left && OutputDir == GridManager.Direction.Down)
            {
                Texture = Curve1;
            }
            if (InputDir == GridManager.Direction.Up && OutputDir == GridManager.Direction.Left)
            {
                Texture = Curve1;
                RotationDegrees = 90;
            }
            if (InputDir == GridManager.Direction.Right && OutputDir == GridManager.Direction.Up)
            {
                Texture = Curve1;
                RotationDegrees = 180;
            }
            if (InputDir == GridManager.Direction.Down && OutputDir == GridManager.Direction.Right)
            {
                Texture = Curve1;
                RotationDegrees = 270;
            }

            if (InputDir == GridManager.Direction.Down && OutputDir == GridManager.Direction.Left)
            {
                Texture = Curve2;
            }
            if (InputDir == GridManager.Direction.Left && OutputDir == GridManager.Direction.Up)
            {
                Texture = Curve2;
                RotationDegrees = 90;
            }
            if (InputDir == GridManager.Direction.Up && OutputDir == GridManager.Direction.Right)
            {
                Texture = Curve2;
                RotationDegrees = 180;
            }
            if (InputDir == GridManager.Direction.Right && OutputDir == GridManager.Direction.Down)
            {
                Texture = Curve2;
                RotationDegrees = 270;
            }
        }

        public static GridManager.Direction GetDirectionFromIndices(int selfIndex, int targetIndex)
        {
            int diff = targetIndex - selfIndex;
            switch (diff)
            {
                case 1:
                    return GridManager.Direction.Right;
                case -1:
                    return GridManager.Direction.Left;
                case 15:
                    return GridManager.Direction.Down;
                case -15:
                    return GridManager.Direction.Up;
            }

            throw new Exception($"target {targetIndex} is not next to self {selfIndex}");
        }
    }
}