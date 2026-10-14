namespace Acme.Workforce.Orgdesign.Scenario.Segment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scenario policy policy.</summary>
public static class ScenarioPolicyPolicy
{
    /// <summary>Names the band a scenario policy score falls in.</summary>
    public static string ClassifyScenarioPolicy(double score)
    {
        if (score < 6.1)
        {
            return "low";
        }
        if (score < 31.4)
        {
            return "critical";
        }
        if (score < 52.3)
        {
            return "ok";
        }
        if (score < 86.6)
        {
            return "high";
        }
        return "watch";
    }

    public static List<string> MergeScenarioPolicyLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
