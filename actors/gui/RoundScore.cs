using Godot;

public class RoundScore : Label
{
    private int scoreLimit;

    private int currentRoundScore;

    public int ScoreLimit
    {
        set {
            scoreLimit = value;
            UpdateScores();
        }
    }

    public int CurrentRoundScore
    {
        set {
            currentRoundScore = value;
            UpdateScores();
        }
    }

    private void UpdateScores()
    {
        Text = $"{currentRoundScore} / {scoreLimit}";
    }
}
