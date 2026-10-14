namespace Acme.Workforce.Workplace.FloorPlan.Entry;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of floor plan entry policy.</summary>
public static class FloorPlanEntryPolicy
{
    public static int FloorPlanEntrySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }

    public static double FloorPlanEntryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.59;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFloorPlanEntry()
    {
        var policy = Retry.DefaultPolicy("floor-plan-entry");
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

    /// <summary>Names the band a floor plan entry score falls in.</summary>
    public static string ClassifyFloorPlanEntry(double score)
    {
        if (score < 16.0)
        {
            return "watch";
        }
        if (score < 55.7)
        {
            return "elevated";
        }
        if (score < 73.4)
        {
            return "high";
        }
        return "ok";
    }
}
