namespace Acme.Workforce.Engagement.TownHall.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy town hall score policy.</summary>
public static class LegacyTownHallScorePolicy
{
    public static string LegacyTownHallScoreBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static double LegacyTownHallScoreWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.83;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a legacy town hall score code in its canonical LTHS- form.</summary>
    public static string NormalizeLegacyTownHallScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LTHS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LTHS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a legacy town hall score score falls in.</summary>
    public static string ClassifyLegacyTownHallScore(double score)
    {
        if (score < 40.9)
        {
            return "elevated";
        }
        if (score < 58.2)
        {
            return "ok";
        }
        return "high";
    }
}
