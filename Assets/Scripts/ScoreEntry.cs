using UnityEngine;

[System.Serializable]
public class ScoreEntry
{
    public string date;
    public int level;
    public int scorePercent;
    public float timeTaken;

    public ScoreEntry(string date, int level, int scorePercent, float timeTaken)
    {
        this.date = date;
        this.level = level;
        this.scorePercent = scorePercent;
        this.timeTaken = timeTaken;
    }
}

