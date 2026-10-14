namespace Acme.Workforce.Engagement.TownHall.HallException;

/// <summary>Functions of town hall baseline policy.</summary>
public static class TownHallBaselinePolicy
{
    /// <summary>Names the band a town hall baseline score falls in.</summary>
    public static string ClassifyTownHallBaseline(double score)
    {
        if (score < 20.3)
        {
            return "critical";
        }
        if (score < 59.0)
        {
            return "high";
        }
        return "ok";
    }

    public static int TownHallBaselineSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }
}
