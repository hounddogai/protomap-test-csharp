namespace Acme.Workforce.Contractors.Rate.RateCycle;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of capped rate metric policy.</summary>
public static class CappedRateMetricPolicy
{
    public static int CappedRateMetricSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedRateMetric()
    {
        var policy = Retry.DefaultPolicy("capped-rate-metric");
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
