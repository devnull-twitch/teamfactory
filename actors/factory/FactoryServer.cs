using Godot;
using Godot.Collections;
using TeamFactory.Map;
using TeamFactory.Items;
using TeamFactory.Lib.Multiplayer;

namespace TeamFactory.Factory
{
    public class FactoryServer : Reference, IServer
    {
        private FactoryNode node;

        private TileResource tileResource;

        private float cooldown;

        private Dictionary<string, int> storage = new Dictionary<string, int>();

        public FactoryServer(FactoryNode node, TileResource tileResource)
        {
            this.node = node;
            this.tileResource = tileResource;

            cooldown = tileResource.SpawnInterval;
        }

        public void ClientRequest(string method, params object[] args)
        {
            throw new System.NotImplementedException();
        }

        public void Tick(float delta)
        {
            cooldown -= delta;
            if (cooldown <= 0 && RequiredmentsCheck())
            {
                PopFromStorage();
                cooldown = tileResource.SpawnInterval;

                node.SpawnItem();
                node.ServerSend("SpawnItem");
            }
        }

        public void ItemArrived(ItemNode itemNode)
        {
            
            if (!storage.ContainsKey(itemNode.Item.Name))
            {
                storage.Add(itemNode.Item.Name, 1);
                return;
            }

            storage[itemNode.Item.Name] += 1;
        }

        private bool RequiredmentsCheck()
        {
            foreach(System.Collections.Generic.KeyValuePair<string, int> tuple in tileResource.SpawnResource.Requirements)
            {
                if (!storage.ContainsKey(tuple.Key))
                {
                    return false;
                }

                if (storage[tuple.Key] < tuple.Value)
                {
                    return false;
                }
            }

            return true;
        }

        private void PopFromStorage()
        {
            foreach(System.Collections.Generic.KeyValuePair<string, int> tuple in tileResource.SpawnResource.Requirements)
            {
                storage[tuple.Key] -= tuple.Value;
            }
        }
    }
}