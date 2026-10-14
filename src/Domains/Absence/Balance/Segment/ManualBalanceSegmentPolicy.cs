namespace Acme.Workforce.Absence.Balance.Segment;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of manual balance segment policy.</summary>
public static class ManualBalanceSegmentPolicy
{
    public static double ManualBalanceSegmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.89;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a manual balance segment score falls in.</summary>
    public static string ClassifyManualBalanceSegment(double score)
    {
        if (score < 22.4)
        {
            return "elevated";
        }
        if (score < 40.1)
        {
            return "high";
        }
        if (score < 59.4)
        {
            return "low";
        }
        return "ok";
    }

    public static int ManualBalanceSegmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeManualBalanceSegment()
    {
        var policy = Retry.DefaultPolicy("manual-balance-segment");
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
}
