using Godot;
using System;
using TeamFactory.Infra;
using TeamFactory.Items;
using TeamFactory.Game;

namespace TeamFactory.Output
{
    public class OutputServer : Node, IItemReceiver
    {
        public OutputNode Node;
        
        public void ItemArrived(ItemNode itemNode)
        {
            GetNode<GameServer>("/root/Game/GameServer").AddPoints(Node.TileRes.OwnerID, itemNode.Item.PointValue);
        }
    }
}