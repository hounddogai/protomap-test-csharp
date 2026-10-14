namespace Acme.Workforce.Workplace.Occupancy.Target;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of strict occupancy segment policy.</summary>
public static class StrictOccupancySegmentPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictOccupancySegment()
    {
        var policy = Retry.DefaultPolicy("strict-occupancy-segment");
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

    /// <summary>Names the band a strict occupancy segment score falls in.</summary>
    public static string ClassifyStrictOccupancySegment(double score)
    {
        if (score < 10.5)
        {
            return "minimal";
        }
        if (score < 15.3)
        {
            return "watch";
        }
        return "low";
    }
}
