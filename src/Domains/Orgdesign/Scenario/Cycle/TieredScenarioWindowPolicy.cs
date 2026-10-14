namespace Acme.Workforce.Orgdesign.Scenario.Cycle;

/// <summary>Functions of tiered scenario window policy.</summary>
public static class TieredScenarioWindowPolicy
{
    public static int TieredScenarioWindowSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Names the band a tiered scenario window score falls in.</summary>
    public static string ClassifyTieredScenarioWindow(double score)
    {
        if (score < 34.0)
        {
            return "critical";
        }
        if (score < 39.1)
        {
            return "watch";
        }
        if (score < 86.0)
        {
            return "minimal";
        }
        return "low";
    }
}
