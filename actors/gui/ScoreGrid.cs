using Godot;
using Godot.Collections;
using TeamFactory.Player;
using TeamFactory.Util.Multiplayer;

namespace TeamFactory.Gui
{
    public class ScoreGrid : GridContainer
    {
        private Dictionary<string, ProgressBar> pointLabelMap = new Dictionary<string, ProgressBar>();

        private PackedScene ScoreEntryScene;

        public int ScoreLimit;

        public override void _Ready()
        {
            ScoreEntryScene = GD.Load<PackedScene>("res://actors/gui/ScoreEntry.tscn");
        }

        public void SetScore(int playerID, string playerName, int points, int roundPoints)
        {
            if (!pointLabelMap.ContainsKey(playerName)) 
            {
                Button playerNameBtn = new Button();
                if (NetState.NetworkId(this) == playerID)
                    playerNameBtn.Text = $"{playerName} ( You )";
                else
                    playerNameBtn.Text = playerName;
                Array funcArgs = new Array();
                funcArgs.Add(playerID);
                playerNameBtn.Connect("pressed", this, "JumpToPlayer", funcArgs);
                AddChild(playerNameBtn);
                
                ProgressBar scoreContainer = CreateWithText(points);
                pointLabelMap[playerName] = scoreContainer;
                AddChild(scoreContainer);
            }
            else
            {
                pointLabelMap[playerName].Value = points;
                pointLabelMap[playerName].MaxValue = ScoreLimit;
                pointLabelMap[playerName].GetNode<Label>("Label").Text = $"{points}";
            }
        }

        private ProgressBar CreateWithText(int points)
        {
            ProgressBar pointProgress = ScoreEntryScene.Instance<ProgressBar>();
            pointProgress.MaxValue = ScoreLimit;
            pointProgress.Value = points;
            pointProgress.GetNode<Label>("Label").Text = $"{points}";

            return pointProgress;
        }

        public void JumpToPlayer(int playerID)
        {
            PlayerNode selfPlayerNode = GetNode<PlayerNode>($"/root/Game/Players/{NetState.NetworkId(this)}");
            if (playerID == NetState.NetworkId(this))
            {
                selfPlayerNode.ResetCamera();
                return;
            }

            Vector2 jumpTarget = GetNode<PlayerNode>($"/root/Game/Players/{playerID}").GlobalPosition;
            selfPlayerNode.JumpCameraTo(jumpTarget);
        }
    }
}