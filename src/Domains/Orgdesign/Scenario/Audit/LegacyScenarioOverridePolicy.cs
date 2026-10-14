namespace Acme.Workforce.Orgdesign.Scenario.Audit;

/// <summary>Functions of legacy scenario override policy.</summary>
public static class LegacyScenarioOverridePolicy
{
    /// <summary>Puts a legacy scenario override code in its canonical LSO- form.</summary>
    public static string NormalizeLegacyScenarioOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LSO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LSO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a legacy scenario override score falls in.</summary>
    public static string ClassifyLegacyScenarioOverride(double score)
    {
        if (score < 25.9)
        {
            return "ok";
        }
        if (score < 77.6)
        {
            return "minimal";
        }
        if (score < 85.5)
        {
            return "critical";
        }
        if (score < 98.6)
        {
            return "high";
        }
        return "low";
    }

    public static int LegacyScenarioOverrideSpan(int start, int end)
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
