using Godot;
using Godot.Collections;

namespace TeamFactory.Gui
{
    public class ScoreGrid : GridContainer
    {
        private Dictionary<string, Label> pointLabelMap = new Dictionary<string, Label>();

        private PackedScene ScoreEntryScene;

        public override void _Ready()
        {
            ScoreEntryScene = GD.Load<PackedScene>("res://actors/gui/ScoreEntry.tscn");
        }

        public void SetScore(string playerName, int points)
        {
            if (!pointLabelMap.ContainsKey(playerName)) 
            {
                AddChild(CreateWithText(playerName));
                Control scoreContainer = CreateWithText($"{points}");
                pointLabelMap[playerName] = scoreContainer.GetNode<Label>("Label");
                AddChild(scoreContainer);
            }
            else
            {
                pointLabelMap[playerName].Text = $"{points}";
            }
        }

        private Control CreateWithText(string text)
        {
            Control newLabel = ScoreEntryScene.Instance<Control>();
            newLabel.GetNode<Label>("Label").Text = text;

            return newLabel;
        }
    }
}