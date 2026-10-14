namespace Acme.Workforce.Engagement.TownHall.Notice;

/// <summary>Functions of weighted town hall exception policy.</summary>
public static class WeightedTownHallExceptionPolicy
{
    /// <summary>Puts a weighted town hall exception code in its canonical WTHE- form.</summary>
    public static string NormalizeWeightedTownHallExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WTHE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WTHE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a weighted town hall exception score falls in.</summary>
    public static string ClassifyWeightedTownHallException(double score)
    {
        if (score < 7.2)
        {
            return "elevated";
        }
        if (score < 32.3)
        {
            return "minimal";
        }
        if (score < 49.1)
        {
            return "ok";
        }
        if (score < 89.3)
        {
            return "medium";
        }
        return "watch";
    }
}
