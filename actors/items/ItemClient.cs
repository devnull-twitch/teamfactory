using TeamFactory.Lib.Multiplayer;

namespace TeamFactory.Items
{
    public class ItemClient : IClient
    {
        private ItemNode node;

        public ItemClient(ItemNode node)
        {
            this.node = node;
        }

        public void ServerRequest(string method, params object[] args)
        {
            throw new System.NotImplementedException();
        }
    }
}