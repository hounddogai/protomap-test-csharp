namespace Acme.Workforce.Talent.Pool.Adjustment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of manual pool tier policy.</summary>
public static class ManualPoolTierPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeManualPoolTier()
    {
        var policy = Retry.DefaultPolicy("manual-pool-tier");
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

    public static int ManualPoolTierSpan(int start, int end)
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
}
