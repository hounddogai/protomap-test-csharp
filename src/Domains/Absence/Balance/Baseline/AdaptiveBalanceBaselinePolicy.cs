namespace Acme.Workforce.Absence.Balance.Baseline;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive balance baseline policy.</summary>
public static class AdaptiveBalanceBaselinePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveBalanceBaseline()
    {
        var policy = Retry.DefaultPolicy("adaptive-balance-baseline");
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

    public static int AdaptiveBalanceBaselineSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }
}
