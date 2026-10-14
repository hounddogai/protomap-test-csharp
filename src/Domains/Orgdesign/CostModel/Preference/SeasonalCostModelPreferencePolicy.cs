namespace Acme.Workforce.Orgdesign.CostModel.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal cost model preference policy.</summary>
public static class SeasonalCostModelPreferencePolicy
{
    public static double SeasonalCostModelPreferenceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.31;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string SeasonalCostModelPreferenceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a seasonal cost model preference code in its canonical SCMP- form.</summary>
    public static string NormalizeSeasonalCostModelPreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SCMP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SCMP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
