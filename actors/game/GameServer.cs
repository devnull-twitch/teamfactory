using Godot;
using Godot.Collections;
using TeamFactory.Lib.Multiplayer;
using TeamFactory.Player;
using TeamFactory.Map;

namespace TeamFactory.Game
{
    public class GameServer : Node
    {
        private GameNode node;

        private Dictionary<int, string> players = new Dictionary<int, string>();

        protected Dictionary<int, int> UserPoints = new Dictionary<int, int>();

        private PackedScene playerPackaged;

        public float TimeTillNextRound;

        public override void _Process(float delta)
        {
            if (NetState.Mode != Mode.NET_SERVER)
            {
                return;
            }

            TimeTillNextRound -= delta;
            if (TimeTillNextRound <= 0)
            {
                NetState.Rpc(GetNode<MapNode>("../GridManager"), "NextRound");
            }
        }

        public void AddPoints(int ownerID, int points)
        {
            if (!UserPoints.ContainsKey(ownerID))
            {
                UserPoints[ownerID] = 0;
            }

            if (node == null)
            {
                node = GetNode<GameNode>("..");
            }

            UserPoints[ownerID] = UserPoints[ownerID] + points;
            NetState.Rpc(node, "SetPoints", ownerID, UserPoints[ownerID]);
        }

        public void AddPlayer(int ownerID, string playerName)
        {
            if (playerPackaged == null)
            {
                playerPackaged = GD.Load<PackedScene>("res://actors/player/Player.tscn");
            }

            PlayerNode newPlayerNode = playerPackaged.Instance<PlayerNode>();
            newPlayerNode.OwnerID = ownerID;
            newPlayerNode.PlayerName = playerName;
            newPlayerNode.Name = $"{ownerID}";

            players[ownerID] = playerName;
            GetNode<Node>("../Players").AddChild(newPlayerNode);
        }

        [Remote]
        public void requestClientInit()
        {
            if (node == null)
            {
                node = GetNode<GameNode>("..");
            }

            foreach(int netID in players.Keys)
            {
                NetState.RpcId(node, NetState.NetworkSenderId(this), "AddPlayer", netID, players[netID]);
            }
        }

        public System.Collections.Generic.ICollection<int> GetPlayerIDs()
        {
            return players.Keys;
        }
    }
}