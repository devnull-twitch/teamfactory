using System;
using Godot;
using Godot.Collections;
using TeamFactory.Util.Multiplayer;
using TeamFactory.Player;
using TeamFactory.Map;
using TeamFactory.Items;
using TeamFactory.Powerplant;
using TeamFactory.Infra;

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

        public float TimerTillNextEnergy;

        protected Dictionary<int, int> UserRoundPoints = new Dictionary<int, int>();

        protected Dictionary<int, Dictionary<SabotageType, int>> sabotageRoundUsages = new Dictionary<int, Dictionary<SabotageType, int>>();

        protected Dictionary<int, PlayerPower> playerPowers = new Dictionary<int, PlayerPower>();

        protected Dictionary<int, float> playerPowerCostMultiplier = new Dictionary<int, float>();

        public Dictionary<int, int> UserInfraTokens = new Dictionary<int, int>();

        public int ScoreLimit;

        public override void _Ready()
        {
            if (NetState.Mode == Mode.NET_CLIENT)
                return;

            Rng.Seed = (ulong)(DateTime.Now.Ticks);

            GetTree().Connect("network_peer_disconnected", this, nameof(onNetworkPeerDisconnected));
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

            TimerTillNextEnergy -= delta;
            if (TimerTillNextEnergy <= 0)
            {
                TriggerDefaultEnergyProduction();
                TimerTillNextEnergy = 1f;
            }
                
        }

        public void onNetworkPeerDisconnected(int peerID)
        {
            players.Remove(peerID);
            playerPowers.Remove(peerID);
            UserPoints.Remove(peerID);
            UserRoundPoints.Remove(peerID);

            NetState.Rpc(node, "PlayerLeft", peerID);

            if (players.Count <= 0)
                GetTree().Quit();
        }

        [Remote]
        public void GameEnd(string winnerName)
        {
            GetTree().NetworkPeer = null;
            GetNode<Label>("/root/Game/HUD/GameEndDialog/CenterContainer/VBoxContainer/Winner").Text = winnerName;
            GetNode<PopupDialog>("/root/Game/HUD/GameEndDialog").Popup_();
            GetNode<GameNode>("..").GameRunning = false;
        }

        public void AddPoints(int ownerID, int points)
        {
            if (!UserPoints.ContainsKey(ownerID))
                return;

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

        public void AddPower(int ownerID, int points)
        {
            if (!playerPowers.ContainsKey(ownerID))
            {
                playerPowers[ownerID] = new PlayerPower();
                playerPowers[ownerID].MaxValue = 100;
            }

            playerPowers[ownerID].Add(points);
            NetState.RpcId(node, ownerID, "SetPower", playerPowers[ownerID].Current, playerPowers[ownerID].MaxValue);
        }

        public int GetPlayerPower(int playerID)
        {
            if (!playerPowers.ContainsKey(playerID))
                return 0;

            return playerPowers[playerID].Current;
        }

        public bool ReducePlayerPower(int playerID, int points)
        {
            if (points <= 0)
                return true;

            if (!playerPowers.ContainsKey(playerID))
                return false;

            points = (int)Math.Floor((float)points * getPowerCostMultiplier(playerID));

            if (playerPowers[playerID].Current < points)
                return false;

            playerPowers[playerID].Current -= points;
            NetState.RpcId(node, playerID, "SetPower", playerPowers[playerID].Current, playerPowers[playerID].MaxValue);
            return true;
        }

        private void TriggerNextRound()
        {
            GD.Print("moving along to next round");
            if (!GetNode<MapNode>("../GridManager").NextRound())
            {
                string winnerName = "";
                int winningPoints = 0;
                foreach (int netID in players.Keys)
                {
                    if (UserPoints[netID] > winningPoints)
                    {
                        winnerName = players[netID];
                        winningPoints = UserPoints[netID];
                    }
                }

                NetState.Rpc(this, "GameEnd", winnerName);
                GetTree().Quit();
            }
        }

        private void TriggerDefaultEnergyProduction()
        {
            foreach(int playerNetID in players.Keys)
            {
                AddPower(playerNetID, 2);
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
                UserRoundPoints[playerID] = 0;
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
            UserPoints[ownerID] = 0;
            GetNode<Node>("../Players").AddChild(newPlayerNode);
        }

        public void DisableNode(int nodeIndex)
        {
            MapNode mapNode = GetNode<MapNode>("/root/Game/GridManager");
            InfraSprite infraNode = mapNode.Manager.GetInfraAtIndex(nodeIndex);
            NetState.Rpc(infraNode, "TriggerDisable");
        }

        public void EnableNode(int nodeIndex)
        {
            MapNode mapNode = GetNode<MapNode>("/root/Game/GridManager");
            InfraSprite infraNode = mapNode.Manager.GetInfraAtIndex(nodeIndex);
            NetState.Rpc(infraNode, "TriggerEnable");
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

            NetState.RpcId(node, NetState.NetworkSenderId(this), "InitPlayerFinished");

            playersLoaded[NetState.NetworkSenderId(this)] = true;
            if (playersLoaded.Count == players.Count)
            {
                GetTree().CallGroup("spawners", "PlayersLoaded");
                gameRunning = true;
            }
        }

        [Remote]
        public void RequestEnable(int nodeIndex)
        {
            // TODO: Validate player position relative to map index
            EnableNode(nodeIndex);
        }

        public Array<string> GetPlayerUnlocks(int playerID)
        {
            if (!playerUnlocks.ContainsKey(playerID))
                return null;
                
            return playerUnlocks[playerID];
        }

        [Remote]
        public void RequestSabotage(SabotageType sType, int targetID)
        {
            GD.Print("received sabotage request");
            if (targetID == 0)
                targetID = getRandomPlayerID(NetState.NetworkSenderId(this));

            if (targetID == 0)
            {
                GD.Print("Not enough players to sabotage");
                return;
            }

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
            sabotageObj.Execute(targetID);
            NetState.RpcId(node, NetState.NetworkSenderId(this), "SabotageExecuted", sType);
        }

        private int getRandomPlayerID(int excludeID)
        {
            Array<int> possibleTagetPlayerIDs = new Array<int>();
            foreach (int netID in players.Keys)
                if (netID != excludeID)
                    possibleTagetPlayerIDs.Add(netID);

            if (possibleTagetPlayerIDs.Count <= 0)
                return 0;

            int victomIndex = Rng.RandiRange(0, possibleTagetPlayerIDs.Count - 1);
            return possibleTagetPlayerIDs[victomIndex];
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

        public void SetInfraTokensForAll(int tokenCount)
        {
            foreach(int netID in players.Keys)
            {
                UserInfraTokens[netID] = tokenCount;
                NetState.RpcId(node, netID, "SetInfraTokens", tokenCount);
            }
        }

        public void SubInfraToken(int netID)
        {
            if (!UserInfraTokens.ContainsKey(netID))
                return;

            UserInfraTokens[netID]--;
            NetState.RpcId(node, netID, "SetInfraTokens", UserInfraTokens[netID]);
        }

        public void IncInfraToken(int netID)
        {
            if (!UserInfraTokens.ContainsKey(netID))
                return;

            UserInfraTokens[netID]++;
            NetState.RpcId(node, netID, "SetInfraTokens", UserInfraTokens[netID]);
        }

        [Remote]
        public void RequestUnlock(string itemName)
        {
            int senderNetId = NetState.NetworkSenderId(this);
            ItemDB itemDB = GD.Load<ItemDB>("res://actors/items/ItemDB.tres");
            ItemResource unlockItem = itemDB.Database[itemName];

            if (!UserPoints.ContainsKey(senderNetId))
                UserPoints[senderNetId] = 0;

            if (UserPoints[senderNetId] < unlockItem.UnlockCost)
                return;

            UserPoints[senderNetId] -= unlockItem.UnlockCost;
            NetState.Rpc(node, "SetPoints", senderNetId, UserPoints[senderNetId], UserRoundPoints[senderNetId]);

            playerUnlocks[senderNetId].Add(itemName);
            NetState.RpcId(node, senderNetId, "AddPlayerUnlock", itemName);
        }

        public System.Collections.Generic.ICollection<int> GetPlayerIDs()
        {
            return players.Keys;
        }

        public void TriggerFlipPlayerView(int targetNetID)
        {
            NetState.RpcId(node, targetNetID, "FlipPlayerView");
        }

        public void TriggerResetPlayerView(int targetNetID)
        {
            NetState.RpcId(node, targetNetID, "ResetPlayerView");
        }

        public void SetPowerCostMultiplier(int targetNetID, float multiplyer)
        {
            GD.Print($"set power cost for {targetNetID} to {multiplyer}");
            playerPowerCostMultiplier[targetNetID] = multiplyer;
        }

        private float getPowerCostMultiplier(int targetNetID)
        {
            float m = 0;
            if (!playerPowerCostMultiplier.TryGetValue(targetNetID, out m))
                return 1f;

            return m;
        }
    }
}