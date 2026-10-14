namespace Acme.Workforce.Engagement.TownHall.Adjustment;

/// <summary>Functions of town hall estimate policy.</summary>
public static class TownHallEstimatePolicy
{
    /// <summary>Puts a town hall estimate code in its canonical THE- form.</summary>
    public static string NormalizeTownHallEstimateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("THE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"THE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int TownHallEstimateSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Names the band a town hall estimate score falls in.</summary>
    public static string ClassifyTownHallEstimate(double score)
    {
        if (score < 9.7)
        {
            return "minimal";
        }
        if (score < 16.1)
        {
            return "high";
        }
        if (score < 27.1)
        {
            return "low";
        }
        if (score < 54.4)
        {
            return "medium";
        }
        return "watch";
    }
}
