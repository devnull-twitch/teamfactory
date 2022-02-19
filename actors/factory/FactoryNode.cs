using Godot;
using TeamFactory.Infra;
using TeamFactory.Items;

namespace TeamFactory.Factory
{
    public class FactoryNode : InfraSprite
    {
        private FactoryServer server;

        private FactoryClient client;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            server = new FactoryServer(this, TileRes);
            client = new FactoryClient(this, server);

            GetNode<Area2D>("Picker").Connect("input_event", this, nameof(OnInput));
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

        public void OnInput(Node viewport, InputEvent e, int shape_idx)
        {
            if ( e is InputEventMouseButton eventMouseButton && 
                eventMouseButton.ButtonIndex == (int)ButtonList.Left &&
                eventMouseButton.Pressed == true)
            {
                return;
            }

            GetNode<Area2D>("Picker")._InputEvent(viewport, e, shape_idx);
        }
    }
}