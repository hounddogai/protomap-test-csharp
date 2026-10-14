namespace Acme.Workforce.Workplace.FloorPlan.Metric;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of fallback floor plan metric policy.</summary>
public static class FallbackFloorPlanMetricPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackFloorPlanMetric()
    {
        var policy = Retry.DefaultPolicy("fallback-floor-plan-metric");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a fallback floor plan metric score falls in.</summary>
    public static string ClassifyFallbackFloorPlanMetric(double score)
    {
        if (score < 21.0)
        {
            return "critical";
        }
        if (score < 63.6)
        {
            return "minimal";
        }
        if (score < 84.3)
        {
            return "high";
        }
        return "elevated";
    }

    public static string FallbackFloorPlanMetricBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
