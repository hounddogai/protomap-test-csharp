namespace Acme.Workforce.Orgdesign.Scenario.Tier;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of seasonal scenario tier policy.</summary>
public static class SeasonalScenarioTierPolicy
{
    public static double SeasonalScenarioTierWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.35;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int SeasonalScenarioTierSpan(int start, int end)
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
        return span / 86400;
    }

    public static List<string> MergeSeasonalScenarioTierLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a seasonal scenario tier code in its canonical SST- form.</summary>
    public static string NormalizeSeasonalScenarioTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SST-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SST-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
