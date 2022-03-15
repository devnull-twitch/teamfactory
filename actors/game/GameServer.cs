using System;
using Godot;
using Godot.Collections;
using TeamFactory.Util.Multiplayer;
using TeamFactory.Player;
using TeamFactory.Map;

namespace TeamFactory.Game
{
    public class GameServer : Node
    {
        private GameNode node;

        private Dictionary<int, string> players = new Dictionary<int, string>();

        protected Dictionary<int, int> UserPoints = new Dictionary<int, int>();

        private Dictionary<int, bool> playersLoaded = new Dictionary<int, bool>();

        public RandomNumberGenerator Rng = new RandomNumberGenerator();

        private PackedScene playerPackaged;

        private bool gameRunning = false;

        public float TimeTillNextRound;

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
            {
                if (!GetNode<MapNode>("../GridManager").NextRound())
                {
                    NetState.Rpc(this, "GameEnd");
                    GetTree().Quit();
                }
            }
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
                throw new System.Exception("Not enough players to execute sabotage");

            int victomIndex = Rng.RandiRange(0, possibleTagetPlayerIDs.Count - 1);

            Sabotage sabotageObj = Sabotage.GetSabotage(sType, this);
            if (!UserPoints.ContainsKey(NetState.NetworkSenderId(this)))
                return;
            if (UserPoints[NetState.NetworkSenderId(this)] < sabotageObj.PointsCost)
                return;

            UserPoints[NetState.NetworkSenderId(this)] -= sabotageObj.PointsCost;
            NetState.Rpc(node, "SetPoints", NetState.NetworkSenderId(this), UserPoints[NetState.NetworkSenderId(this)]);
            sabotageObj.Execute(possibleTagetPlayerIDs[victomIndex]);
        }

        public System.Collections.Generic.ICollection<int> GetPlayerIDs()
        {
            return players.Keys;
        }
    }
}