namespace Acme.Workforce.Engagement.TownHall.Notice;

/// <summary>Functions of adaptive town hall manifest policy.</summary>
public static class AdaptiveTownHallManifestPolicy
{
    /// <summary>Names the band a adaptive town hall manifest score falls in.</summary>
    public static string ClassifyAdaptiveTownHallManifest(double score)
    {
        if (score < 38.9)
        {
            return "minimal";
        }
        if (score < 73.6)
        {
            return "elevated";
        }
        return "medium";
    }

    public static int AdaptiveTownHallManifestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }
}
