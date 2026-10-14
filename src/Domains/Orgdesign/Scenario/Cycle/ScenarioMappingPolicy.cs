namespace Acme.Workforce.Orgdesign.Scenario.Cycle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scenario mapping policy.</summary>
public static class ScenarioMappingPolicy
{
    public static int ScenarioMappingSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    public static string ScenarioMappingBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Names the band a scenario mapping score falls in.</summary>
    public static string ClassifyScenarioMapping(double score)
    {
        if (score < 11.2)
        {
            return "watch";
        }
        if (score < 90.7)
        {
            return "critical";
        }
        return "medium";
    }
}
