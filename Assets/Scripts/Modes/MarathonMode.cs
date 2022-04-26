using Discord;
using Unity.Collections;
using TMPro;

public class MarathonMode : DummyMode
{
    private int lines;
    private int score;
    private bool b2b;
    private TextMeshPro _text;
    private static readonly int[] lineScoreCount = { 100, 200, 400, 800, 1200, 1600, 2000, 2500 };
    public override FixedString64Bytes Name { get; set; } = "150 Lines Marathon";

    public override FixedString128Bytes Description { get; set; } = "A standard mode. Clear 150 lines!";
    public override Activity GetDiscordActivity() => new Activity
    {
        State = "Playing Marathon.",
        Details = $"Cleared {lines}/150 lines",
        Assets = {
                LargeImage = "icon"
            }
    };
    public override void OnLineClear(NetworkBoard boardRef, int lines, bool spin)
    {
        this.lines += lines;
        score += lineScoreCount[lines];
        if (this.lines >= 150)
        {
            boardRef.ending = true;
            boardRef.GameOver = true;
        }
    }
    public override double GetLineDropDelay()
    {
        return 20;
    }
    public override double GetLineSpawnDelay()
    {
        return 10;
    }
}