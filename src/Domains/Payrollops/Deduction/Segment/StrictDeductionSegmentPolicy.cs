namespace Acme.Workforce.Payrollops.Deduction.Segment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of strict deduction segment policy.</summary>
public static class StrictDeductionSegmentPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictDeductionSegment()
    {
        var policy = Retry.DefaultPolicy("strict-deduction-segment");
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

    /// <summary>Names the band a strict deduction segment score falls in.</summary>
    public static string ClassifyStrictDeductionSegment(double score)
    {
        if (score < 6.1)
        {
            return "ok";
        }
        if (score < 29.7)
        {
            return "critical";
        }
        if (score < 54.9)
        {
            return "watch";
        }
        if (score < 85.8)
        {
            return "high";
        }
        return "low";
    }
}
