namespace Acme.Workforce.Workplace.FloorPlan.Rule;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of floor plan rule policy.</summary>
public static class FloorPlanRulePolicy
{
    /// <summary>Names the band a floor plan rule score falls in.</summary>
    public static string ClassifyFloorPlanRule(double score)
    {
        if (score < 26.3)
        {
            return "watch";
        }
        if (score < 42.8)
        {
            return "critical";
        }
        if (score < 47.3)
        {
            return "high";
        }
        return "ok";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFloorPlanRule()
    {
        var policy = Retry.DefaultPolicy("floor-plan-rule");
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

    public static double FloorPlanRuleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.52;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
