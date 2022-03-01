using Godot;
using Godot.Collections;
using TeamFactory.Lib.Multiplayer;
using TeamFactory.Gui;

namespace TeamFactory.Game
{
    public class GameNode : Node2D
    {
        public GameServer Server;

        public int UserPoints;

        protected Dictionary<int, int> Scores = new Dictionary<int, int>();

        public float TimeTillNextRound = 300;

        private Label PointUi;

        private Label TtnrUi;

        private ScoreGrid ScoresUi;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Server = new GameServer();
            Server.Node = this;
            AddChild(Server);

            if (NetState.Mode == Mode.NET_SERVER)
            {
                return;
            }

            TtnrUi = GetNode<Label>("/root/Game/HUD/TopUI/HBoxContainer/RoundTime");
            PointUi = GetNode<Label>("/root/Game/HUD/TopUI/HBoxContainer/Points");
            ScoresUi = GetNode<ScoreGrid>("/root/Game/HUD/GridContainer");
        }

        public override void _Process(float delta)
        {
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

        [RemoteSync]
        public void SetPoints(int ownerID, int points)
        {
            if (ownerID == NetState.NetworkId(this))
            {
                UserPoints = points;
                PointUi.Text = $"{UserPoints}";
            }

            Scores[ownerID] = points;
            ScoresUi.SetScore(ownerID, points);
        }
    }
}

