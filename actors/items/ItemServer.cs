using TeamFactory.Lib.Multiplayer;

namespace TeamFactory.Items
{
    public class ItemServer : IServer
    {
        private bool onRoute = false;

        private float timeToArrive;

        private ItemNode node;

        public ItemServer(ItemNode node)
        {
            this.node = node;
        }

        public void ClientRequest(string method, params object[] args)
        {
            throw new System.NotImplementedException();
        }

        public void Tick(float delta)
        {
            if (!onRoute)
            {
                return;
            }

            throw new System.NotImplementedException();
        }
    }
}