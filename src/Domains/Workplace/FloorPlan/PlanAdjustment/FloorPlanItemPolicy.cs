namespace Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of floor plan item policy.</summary>
public static class FloorPlanItemPolicy
{
    public static int FloorPlanItemSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Names the band a floor plan item score falls in.</summary>
    public static string ClassifyFloorPlanItem(double score)
    {
        if (score < 24.9)
        {
            return "critical";
        }
        if (score < 57.5)
        {
            return "low";
        }
        if (score < 71.2)
        {
            return "watch";
        }
        return "medium";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFloorPlanItem()
    {
        var policy = Retry.DefaultPolicy("floor-plan-item");
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

    public static string FloorPlanItemBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
