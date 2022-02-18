using Godot;
using TeamFactory.Infra;
using TeamFactory.Items;

namespace TeamFactory.Factory
{
    public class FactoryNode : InfraSprite
    {
        private FactoryServer server;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            server = new FactoryServer(this, TileRes);
        }

        public override void ItemArrived(ItemNode itemNode)
        {
            
            if (!TileRes.Storage.ContainsKey(itemNode.Item.Name))
            {
                TileRes.Storage.Add(itemNode.Item.Name, 1);
                return;
            }

            TileRes.Storage[itemNode.Item.Name] += 1;
        }

        public override void _PhysicsProcess(float delta)
        {
            server.Tick(delta);
        }
    }
}