namespace Acme.Workforce.Workplace.FloorPlan.Entry;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of weighted floor plan metric policy.</summary>
public static class WeightedFloorPlanMetricPolicy
{
    public static List<string> MergeWeightedFloorPlanMetricLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a weighted floor plan metric score falls in.</summary>
    public static string ClassifyWeightedFloorPlanMetric(double score)
    {
        if (score < 25.3)
        {
            return "critical";
        }
        if (score < 67.4)
        {
            return "elevated";
        }
        if (score < 91.6)
        {
            return "high";
        }
        return "ok";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedFloorPlanMetric()
    {
        var policy = Retry.DefaultPolicy("weighted-floor-plan-metric");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
