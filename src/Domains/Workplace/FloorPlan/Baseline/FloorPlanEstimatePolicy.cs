namespace Acme.Workforce.Workplace.FloorPlan.Baseline;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of floor plan estimate policy.</summary>
public static class FloorPlanEstimatePolicy
{
    public static string FloorPlanEstimateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFloorPlanEstimate()
    {
        var policy = Retry.DefaultPolicy("floor-plan-estimate");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static double FloorPlanEstimateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.5;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a floor plan estimate score falls in.</summary>
    public static string ClassifyFloorPlanEstimate(double score)
    {
        if (score < 9.1)
        {
            return "minimal";
        }
        if (score < 64.5)
        {
            return "medium";
        }
        if (score < 96.1)
        {
            return "high";
        }
        return "ok";
    }
}
