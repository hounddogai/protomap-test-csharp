namespace Acme.Workforce.Orgdesign.Scenario.Request;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scenario checkpoint policy.</summary>
public static class ScenarioCheckpointPolicy
{
    /// <summary>Names the band a scenario checkpoint score falls in.</summary>
    public static string ClassifyScenarioCheckpoint(double score)
    {
        if (score < 5.5)
        {
            return "low";
        }
        if (score < 54.0)
        {
            return "watch";
        }
        if (score < 54.1)
        {
            return "ok";
        }
        if (score < 98.9)
        {
            return "critical";
        }
        return "minimal";
    }

    public static List<string> MergeScenarioCheckpointLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int ScenarioCheckpointSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    public static string ScenarioCheckpointBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
