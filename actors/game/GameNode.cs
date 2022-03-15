using Godot;
using Godot.Collections;
using TeamFactory.Util.Multiplayer;
using TeamFactory.Gui;
using TeamFactory.Player;
using TeamFactory.Map;

namespace TeamFactory.Game
{
    public class GameNode : Node2D
    {
        public int UserPoints;

        protected Dictionary<int, int> Scores = new Dictionary<int, int>();

        private Dictionary<int, string> players = new Dictionary<int, string>();

        public float TimeTillNextRound = 300;

        private Label PointUi;

        private Label TtnrUi;

        private ScoreGrid ScoresUi;

        private PackedScene playerPackaged;

        public bool GameRunning = true;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            if (NetState.Mode == Mode.NET_SERVER)
            {
                return;
            }

            TtnrUi = GetNode<Label>("/root/Game/HUD/TopUI/HBoxContainer/RoundTime");
            PointUi = GetNode<Label>("/root/Game/HUD/TopUI/HBoxContainer/Points");
            ScoresUi = GetNode<ScoreGrid>("/root/Game/HUD/GridContainer");

            GetNode<Button>("/root/Game/HUD/TopUI/HBoxContainer/Control/SabotageOptionsBtn").Connect("pressed", this, nameof(OpenSabotageOptionWindow));

            if (NetState.Mode == Mode.NET_CLIENT)
            {
                GameServer gsNode = GetNode<GameServer>("GameServer");
                NetState.RpcId(gsNode, 1, "RequestClientInit");
                GD.Print("requesting players");
            }
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
            if (GetNodeOrNull<CanvasLayer>("/root/Game/HUD/SabotageOptionsPanel") != null)
                return;

            PackedScene packedSabotageWindow = GD.Load<PackedScene>("res://actors/game/SabotageWindow.tscn");
            WindowDialog sabotageWindow = packedSabotageWindow.Instance<WindowDialog>();
            
            GetNode<CanvasLayer>("/root/Game/HUD").AddChild(sabotageWindow);
            sabotageWindow.Popup_();
        }

        [Remote]
        public void SetPoints(int ownerID, int points)
        {
            if (ownerID == NetState.NetworkId(this))
            {
                UserPoints = points;
                PointUi.Text = $"{UserPoints}";
            }

            Scores[ownerID] = points;
            ScoresUi.SetScore(players[ownerID], points);
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

            GetNode<Node>("Players").AddChild(newPlayerNode);
        }

        [Remote]
        public void UpdateTimeTillNextRound(float timeTillNextRound)
        {
            TimeTillNextRound = timeTillNextRound;
        }
    }
}

