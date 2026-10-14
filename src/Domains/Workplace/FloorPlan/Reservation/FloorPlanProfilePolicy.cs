namespace Acme.Workforce.Workplace.FloorPlan.Reservation;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of floor plan profile policy.</summary>
public static class FloorPlanProfilePolicy
{
    public static int FloorPlanProfileSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFloorPlanProfile()
    {
        var policy = Retry.DefaultPolicy("floor-plan-profile");
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

    /// <summary>Names the band a floor plan profile score falls in.</summary>
    public static string ClassifyFloorPlanProfile(double score)
    {
        if (score < 2.7)
        {
            return "watch";
        }
        if (score < 60.1)
        {
            return "minimal";
        }
        if (score < 72.3)
        {
            return "elevated";
        }
        if (score < 92.6)
        {
            return "ok";
        }
        return "critical";
    }

    public static string FloorPlanProfileBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
