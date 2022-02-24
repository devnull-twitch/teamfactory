using Godot;
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
            switch (method)
            {
                case "Move":
                    float x = (float)args[0];
                    float y = (float)args[1];
                    if (NetState.Mode == Mode.NET_CLIENT) {
                        node.GlobalPosition = new Vector2(x, y);
                    }
                    break;

                default:
                    throw new System.NotImplementedException();
            }
        }
    }
}