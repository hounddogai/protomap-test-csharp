namespace Acme.Workforce.Engagement.TownHall.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal town hall group policy.</summary>
public static class SeasonalTownHallGroupPolicy
{
    /// <summary>Puts a seasonal town hall group code in its canonical STHG- form.</summary>
    public static string NormalizeSeasonalTownHallGroupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("STHG-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"STHG-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a seasonal town hall group score falls in.</summary>
    public static string ClassifySeasonalTownHallGroup(double score)
    {
        if (score < 7.5)
        {
            return "high";
        }
        if (score < 66.2)
        {
            return "ok";
        }
        return "medium";
    }

    public static string SeasonalTownHallGroupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
