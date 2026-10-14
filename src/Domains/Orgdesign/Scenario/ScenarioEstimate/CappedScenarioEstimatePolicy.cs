namespace Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped scenario estimate policy.</summary>
public static class CappedScenarioEstimatePolicy
{
    public static List<string> MergeCappedScenarioEstimateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int CappedScenarioEstimateSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Names the band a capped scenario estimate score falls in.</summary>
    public static string ClassifyCappedScenarioEstimate(double score)
    {
        if (score < 23.9)
        {
            return "critical";
        }
        if (score < 70.7)
        {
            return "watch";
        }
        if (score < 88.3)
        {
            return "low";
        }
        if (score < 89.6)
        {
            return "medium";
        }
        return "ok";
    }
}
