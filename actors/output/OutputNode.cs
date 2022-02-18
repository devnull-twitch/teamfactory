using Godot;
using System;
using TeamFactory.Infra;
using TeamFactory.Items;

namespace TeamFactory.Output
{
    public class OutputNode : InfraSprite
    {
        public override void ItemArrived(ItemNode itemNode)
        {
            // TODO: add points to team score
        }
    }
}