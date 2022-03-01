using Godot;
using Godot.Collections;

namespace TeamFactory.Gui
{
    public class ScoreGrid : GridContainer
    {
        private Dictionary<int, Label> pointLabelMap = new Dictionary<int, Label>();

        private PackedScene ScoreEntryScene;

        public override void _Ready()
        {
            ScoreEntryScene = GD.Load<PackedScene>("res://actors/gui/ScoreEntry.tscn");
        }

        public void SetScore(int ownerID, int points)
        {
            if (!pointLabelMap.ContainsKey(ownerID)) 
            {
                AddChild(CreateWithText($"{ownerID}"));
                Control scoreContainer = CreateWithText($"{points}");
                pointLabelMap[ownerID] = scoreContainer.GetNode<Label>("Label");
            }
            else
            {
                pointLabelMap[ownerID].Text = $"{points}";
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