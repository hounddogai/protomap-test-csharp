namespace Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scenario target policy.</summary>
public static class ScenarioTargetPolicy
{
    /// <summary>Names the band a scenario target score falls in.</summary>
    public static string ClassifyScenarioTarget(double score)
    {
        if (score < 42.5)
        {
            return "minimal";
        }
        if (score < 43.2)
        {
            return "high";
        }
        if (score < 85.8)
        {
            return "watch";
        }
        if (score < 95.5)
        {
            return "elevated";
        }
        return "ok";
    }

    public static int ScenarioTargetSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }

    public static string ScenarioTargetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
