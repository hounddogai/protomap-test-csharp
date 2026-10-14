namespace Acme.Workforce.Orgdesign.Scenario.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered scenario draft policy.</summary>
public static class TieredScenarioDraftPolicy
{
    /// <summary>Names the band a tiered scenario draft score falls in.</summary>
    public static string ClassifyTieredScenarioDraft(double score)
    {
        if (score < 52.6)
        {
            return "watch";
        }
        if (score < 92.8)
        {
            return "elevated";
        }
        return "medium";
    }

    public static string TieredScenarioDraftBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a tiered scenario draft code in its canonical TSD- form.</summary>
    public static string NormalizeTieredScenarioDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TSD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TSD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double TieredScenarioDraftWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.89;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
