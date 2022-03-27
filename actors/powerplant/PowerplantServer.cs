using Godot;
using System;
using TeamFactory.Infra;
using TeamFactory.Items;
using TeamFactory.Game;

namespace TeamFactory.Powerplant
{
    public class PowerplantServer : Node, IItemReceiver
    {
        public PowerplantNode Node;
        
        public void ItemArrived(ItemNode itemNode)
        {
            GetNode<GameServer>("/root/Game/GameServer").AddPower(Node.OwnerID, itemNode.Item.PowerValue);
        }
    }
}