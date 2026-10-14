namespace Acme.Workforce.Contractors.Rate.Bundle;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of fallback rate allocation policy.</summary>
public static class FallbackRateAllocationPolicy
{
    public static int FallbackRateAllocationSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackRateAllocation()
    {
        var policy = Retry.DefaultPolicy("fallback-rate-allocation");
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
