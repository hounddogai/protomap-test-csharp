namespace Acme.Workforce.Engagement.TownHall.Tier;

/// <summary>Functions of town hall tier policy.</summary>
public static class TownHallTierPolicy
{
    /// <summary>Names the band a town hall tier score falls in.</summary>
    public static string ClassifyTownHallTier(double score)
    {
        if (score < 32.7)
        {
            return "medium";
        }
        if (score < 71.9)
        {
            return "minimal";
        }
        if (score < 96.0)
        {
            return "low";
        }
        if (score < 97.4)
        {
            return "ok";
        }
        return "critical";
    }

    public static int TownHallTierSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }
}
