namespace Acme.Workforce.Orgdesign.Scenario.Summary;

/// <summary>Functions of weighted scenario mapping policy.</summary>
public static class WeightedScenarioMappingPolicy
{
    /// <summary>Puts a weighted scenario mapping code in its canonical WSM- form.</summary>
    public static string NormalizeWeightedScenarioMappingCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WSM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WSM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int WeightedScenarioMappingSpan(int start, int end)
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
}
