namespace Acme.Workforce.Workplace.FloorPlan.Threshold;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of floor plan policy policy.</summary>
public static class FloorPlanPolicyPolicy
{
    public static double FloorPlanPolicyWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.34;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFloorPlanPolicy()
    {
        var policy = Retry.DefaultPolicy("floor-plan-policy");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static string FloorPlanPolicyBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Names the band a floor plan policy score falls in.</summary>
    public static string ClassifyFloorPlanPolicy(double score)
    {
        if (score < 12.3)
        {
            return "high";
        }
        if (score < 79.3)
        {
            return "low";
        }
        return "critical";
    }
}
