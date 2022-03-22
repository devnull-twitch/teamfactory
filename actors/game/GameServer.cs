using System;
using Godot;
using Godot.Collections;
using TeamFactory.Util.Multiplayer;
using TeamFactory.Player;
using TeamFactory.Map;
using TeamFactory.Items;

namespace TeamFactory.Game
{
    public class GameServer : Node
    {
        private GameNode node;

        private Dictionary<int, string> players = new Dictionary<int, string>();

        protected Dictionary<int, int> UserPoints = new Dictionary<int, int>();

        private Dictionary<int, bool> playersLoaded = new Dictionary<int, bool>();

        private Dictionary<int, Array<string>> playerUnlocks = new Dictionary<int, Array<string>>();

        public RandomNumberGenerator Rng = new RandomNumberGenerator();

        private PackedScene playerPackaged;

        private bool gameRunning = false;

        public float TimeTillNextRound;

        protected Dictionary<int, int> UserRoundPoints = new Dictionary<int, int>();

        protected Dictionary<int, Dictionary<SabotageType, int>> sabotageRoundUsages = new Dictionary<int, Dictionary<SabotageType, int>>();

        public int ScoreLimit;

        public override void _Ready()
        {
            if (NetState.Mode == Mode.NET_CLIENT)
                return;

            Rng.Seed = (ulong)(DateTime.Now.Ticks);
        }

        public override void _Process(float delta)
        {
            if (NetState.Mode != Mode.NET_SERVER)
            {
                return;
            }

            if (!gameRunning)
            {
                return;
            }

            TimeTillNextRound -= delta;
            if (TimeTillNextRound <= 0)
                TriggerNextRound();
        }

        [Remote]
        public void GameEnd()
        {
            GetTree().NetworkPeer = null;
            GetNode<PopupDialog>("/root/Game/HUD/GameEndDialog").Popup_();
            GetNode<GameNode>("..").GameRunning = false;
        }

        public void AddPoints(int ownerID, int points)
        {
            if (!UserPoints.ContainsKey(ownerID))
            {
                UserPoints[ownerID] = 0;
            }

            if (!UserRoundPoints.ContainsKey(ownerID))
            {
                UserRoundPoints[ownerID] = 0;
            }

            if (node == null)
            {
                node = GetNode<GameNode>("..");
            }

            UserPoints[ownerID] = UserPoints[ownerID] + points;
            UserRoundPoints[ownerID] = UserRoundPoints[ownerID] + points;
            NetState.Rpc(node, "SetPoints", ownerID, UserPoints[ownerID], UserRoundPoints[ownerID]);

            if (ScoreLimit > 0 && UserRoundPoints[ownerID] >= ScoreLimit)
                TriggerNextRound();
        }

        private void TriggerNextRound()
        {
            if (!GetNode<MapNode>("../GridManager").NextRound())
            {
                NetState.Rpc(this, "GameEnd");
                GetTree().Quit();
            }
        }

        public void NewRoundStart()
        {
            UserRoundPoints.Clear();
            NetState.Rpc(node, "SetScoreLimit", ScoreLimit);
            foreach (int playerID in players.Keys)
            {
                int playerTotalScore = 0;
                UserPoints.TryGetValue(playerID, out playerTotalScore);
                NetState.Rpc(node, "SetPoints", playerID, playerTotalScore, 0);
            }
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
        public void RequestClientInit()
        {
            if (node == null)
            {
                node = GetNode<GameNode>("..");
            }

            foreach(int netID in players.Keys)
            {
                NetState.RpcId(node, NetState.NetworkSenderId(this), "AddPlayer", netID, players[netID]);
            }

            playersLoaded[NetState.NetworkSenderId(this)] = true;
            if (playersLoaded.Count == players.Count)
            {
                GetTree().CallGroup("spawners", "PlayersLoaded");
                gameRunning = true;
            }
        }

        [Remote]
        public void RequestSabotage(SabotageType sType)
        {
            GD.Print("received sabotage request");
            Array<int> possibleTagetPlayerIDs = new Array<int>();
            foreach (int netID in players.Keys)
                if (netID != NetState.NetworkSenderId(this))
                    possibleTagetPlayerIDs.Add(netID);

            if (possibleTagetPlayerIDs.Count <= 0)
            {
                GD.Print("Not enough players to sabotage");
                return;
            }

            int victomIndex = Rng.RandiRange(0, possibleTagetPlayerIDs.Count - 1);

            Sabotage sabotageObj = Sabotage.GetSabotage(sType, this);
            int senderNetId = NetState.NetworkSenderId(this);

            if (!sabotageRoundUsages.ContainsKey(senderNetId))
                sabotageRoundUsages[senderNetId] =  new Dictionary<SabotageType, int>();

            if (!sabotageRoundUsages[senderNetId].ContainsKey(sType))
                sabotageRoundUsages[senderNetId][sType] = 0;

            if (sabotageRoundUsages[senderNetId][sType] >= sabotageObj.RoundUsages)
            {
                GD.Print("sabotage usage limit reached");
                return;
            }

            sabotageRoundUsages[senderNetId][sType]++;

            if (!UserPoints.ContainsKey(senderNetId))
                return;
            if (UserPoints[senderNetId] < sabotageObj.PointsCost)
                return;

            UserPoints[senderNetId] -= sabotageObj.PointsCost;
            NetState.Rpc(node, "SetPoints", senderNetId, UserPoints[senderNetId], UserRoundPoints[senderNetId]);
            sabotageObj.Execute(possibleTagetPlayerIDs[victomIndex]);
            NetState.RpcId(node, NetState.NetworkSenderId(this), "SabotageExecuted", sType);
        }

        public void UnlockForAll(Array<string> itemNames)
        {
            foreach(int netID in players.Keys)
            {
                if (!playerUnlocks.ContainsKey(netID))
                    playerUnlocks[netID] = new Array<string>();

                foreach(string itemName in itemNames)
                {
                    playerUnlocks[netID].Add(itemName);
                    NetState.RpcId(node, netID, "AddPlayerUnlock", itemName);
                }
            }
        }

        [Remote]
        public void RequestUnlock(string itemName)
        {
            ItemDB itemDB = GD.Load<ItemDB>("res://actors/items/ItemDB.tres");
            ItemResource unlockItem = itemDB.Database[itemName];

            if (!UserPoints.ContainsKey(NetState.NetworkSenderId(this)))
                UserPoints[NetState.NetworkSenderId(this)] = 0;

            if (UserPoints[NetState.NetworkSenderId(this)] < unlockItem.UnlockCost)
                return;

            playerUnlocks[NetState.NetworkSenderId(this)].Add(itemName);
            NetState.RpcId(node, NetState.NetworkSenderId(this), "AddPlayerUnlock", itemName);
        }

        public System.Collections.Generic.ICollection<int> GetPlayerIDs()
        {
            return players.Keys;
        }
    }
}