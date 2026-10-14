namespace Acme.Workforce.Workplace.FloorPlan.Adjustment;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of floor plan checkpoint policy.</summary>
public static class FloorPlanCheckpointPolicy
{
    public static string FloorPlanCheckpointBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Names the band a floor plan checkpoint score falls in.</summary>
    public static string ClassifyFloorPlanCheckpoint(double score)
    {
        if (score < 16.8)
        {
            return "minimal";
        }
        if (score < 49.4)
        {
            return "ok";
        }
        if (score < 64.1)
        {
            return "watch";
        }
        if (score < 81.8)
        {
            return "medium";
        }
        return "critical";
    }

    public static double FloorPlanCheckpointWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.98;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFloorPlanCheckpoint()
    {
        var policy = Retry.DefaultPolicy("floor-plan-checkpoint");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
