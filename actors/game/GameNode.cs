using SysGen = System.Collections.Generic;
using Godot;
using Godot.Collections;
using TeamFactory.Util.Multiplayer;
using TeamFactory.Gui;
using TeamFactory.Player;
using TeamFactory.Map;
using TeamFactory.Powerplant;

namespace TeamFactory.Game
{
    public class GameNode : Node2D
    {
        public int UserPoints;

        public int UserRoundPoints;

        protected Dictionary<int, int> Scores = new Dictionary<int, int>();

        protected Dictionary<int, int> RoundScores = new Dictionary<int, int>();

        private Dictionary<int, string> players = new Dictionary<int, string>();

        public float TimeTillNextRound = 300;

        private Label PointUi;

        private Label TtnrUi;

        private RoundScore roundScoreUi;

        private ScoreGrid ScoresUi;

        private PowerUI powerUi;

        private Label tokenLabel;

        private PackedScene playerPackaged;

        public Dictionary<string, bool> PlayerUnlocks;

        public Dictionary<SabotageType, int> SabotageRoundUsages = new Dictionary<SabotageType, int>();

        public bool GameRunning = true;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            if (NetState.Mode == Mode.NET_SERVER)
            {
                return;
            }

            PlayerUnlocks = new Dictionary<string, bool>();

            TtnrUi = GetNode<Label>("/root/Game/HUD/TopUI/HBoxContainer/RoundTime");
            PointUi = GetNode<Label>("/root/Game/HUD/TopUI/HBoxContainer/Points");
            ScoresUi = GetNode<ScoreGrid>("/root/Game/HUD/GridContainer");
            roundScoreUi = GetNode<RoundScore>("HUD/TopUI/HBoxContainer/RoundScore");
            powerUi = GetNode<PowerUI>("/root/Game/HUD/PowerContainer");
            tokenLabel = GetNode<Label>("/root/Game/HUD/TopUI/HBoxContainer/Tokens");

            GetNode<Button>("/root/Game/HUD/TopUI/HBoxContainer/Control/SabotageOptionsBtn").Connect("pressed", this, nameof(OpenSabotageOptionWindow));
            GetNode<Button>("/root/Game/HUD/TopUI/HBoxContainer/Control/UnlockOpenBtn").Connect("pressed", this, nameof(OpenUnlockWindow));

            if (NetState.Mode == Mode.NET_CLIENT)
            {
                GameServer gsNode = GetNode<GameServer>("GameServer");
                NetState.RpcId(gsNode, 1, "RequestClientInit");
                GD.Print("requesting players");
            }
        }

        [Remote]
        public void InitPlayerFinished()
        {
            GetTree().CallGroup("client_players_loaded", "ClientPlayersLoaded");
        }

        public override void _Process(float delta)
        {
            if (!GameRunning)
                return;

            if (NetState.Mode == Mode.NET_SERVER)
            {
                return;
            }

            TimeTillNextRound -= delta;
            int seconds = (int)TimeTillNextRound;
            int minutes = seconds / 60;
            seconds = seconds % 60;
            TtnrUi.Text = $"{minutes}:{seconds}";
        }

        public void OpenSabotageOptionWindow()
        {
            GetNode<SabotageWindow>("HUD/SabotageOptionsPanel").Popup_();
        }

        public void OpenUnlockWindow()
        {
            GetNode<WindowDialog>("HUD/UnlockWindow").Popup_();
        }

        [Remote]
        public void SetPower(int currentPower, int maxPower)
        {
            powerUi.Current = currentPower;
            powerUi.MaxValue = maxPower;
        }

        [Remote]
        public void SetPoints(int ownerID, int points, int roundPoints)
        {
            if (ownerID == NetState.NetworkId(this))
            {
                UserPoints = points;
                UserRoundPoints = roundPoints;
                PointUi.Text = $"{UserPoints}";
            }

            Scores[ownerID] = points;
            RoundScores[ownerID] = roundPoints;
            ScoresUi.SetScore(ownerID, players[ownerID], points, roundPoints);

            roundScoreUi.CurrentRoundScore = UserRoundPoints;
        }

        [Remote]
        public void SetScoreLimit(int scoreLimit)
        {
            roundScoreUi.ScoreLimit = scoreLimit;
            ScoresUi.ScoreLimit = scoreLimit;
        }

        [Remote]
        public void AddPlayer(int ownerID, string name)
        {
            if (playerPackaged == null)
            {
                playerPackaged = GD.Load<PackedScene>("res://actors/player/Player.tscn");
            }

            PlayerNode newPlayerNode = playerPackaged.Instance<PlayerNode>();
            newPlayerNode.OwnerID = ownerID;
            newPlayerNode.PlayerName = name;
            newPlayerNode.Name = $"{ownerID}";

            players[ownerID] = name;
            ScoresUi.SetScore(ownerID, name, 0, 0);

            GetNode<Node>("Players").AddChild(newPlayerNode);
        }

        [Remote]
        public void UpdateTimeTillNextRound(float timeTillNextRound)
        {
            TimeTillNextRound = timeTillNextRound;
        }

        [Remote]
        public void AddPlayerUnlock(string itemName)
        {
            PlayerUnlocks[itemName] = true;
            UnlockWindow unlockWin = GetNode<UnlockWindow>("/root/Game/HUD/UnlockWindow");
            if (unlockWin.Visible)
                unlockWin.OnShow();
        }

        [Remote]
        public void SetInfraTokens(int tokens)
        {
            tokenLabel.Text = $"{tokens}";
        }

        [Remote]
        public void SabotageExecuted(SabotageType sType)
        {
            if (!SabotageRoundUsages.ContainsKey(sType))
                SabotageRoundUsages[sType] = 0;

            SabotageRoundUsages[sType]++;

            GetNode<SabotageWindow>("HUD/SabotageOptionsPanel").OnAboutToShow();
        }

        [Remote]
        public void FlipPlayerView()
        {
            GetNode<Camera2D>($"Players/{NetState.NetworkId(this)}/Camera2D").Zoom = new Vector2(-1.5f, 1.5f);
        }

        [Remote]
        public void ResetPlayerView()
        {
            GetNode<Camera2D>($"Players/{NetState.NetworkId(this)}/Camera2D").Zoom = new Vector2(1.5f, 1.5f);
        }

        [Remote]
        public void PlayerLeft(int peerID)
        {
            players.Remove(peerID);
        }

        public int GetPlayerIDByName(string name)
        {
            if (!players.Values.Contains(name))
                throw new System.Exception("player not found");

            foreach (int playerID in players.Keys)
            {
                if (players[playerID] == name)
                    return playerID;
            }

            throw new System.Exception("player not found ( should never be thrown )");
        }

        public SysGen.ICollection<string> GetPlayerNames()
        {
            return players.Values;
        }
    }
}

