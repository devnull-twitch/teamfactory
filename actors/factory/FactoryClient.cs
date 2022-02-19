
namespace TeamFactory.Factory
{
    public class FactoryClient
    {
        private FactoryNode node;

        private FactoryServer server;

        public FactoryClient(FactoryNode node, FactoryServer server)
        {
            this.node = node;
            this.server = server;
        }

        public void requestInteract()
        {
            
        }
    }
}