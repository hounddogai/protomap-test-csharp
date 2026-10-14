namespace Acme.Workforce.Payrollops.Garnishment.Target;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive garnishment tier policy.</summary>
public static class AdaptiveGarnishmentTierPolicy
{
    public static int AdaptiveGarnishmentTierSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Names the band a adaptive garnishment tier score falls in.</summary>
    public static string ClassifyAdaptiveGarnishmentTier(double score)
    {
        if (score < 74.4)
        {
            return "elevated";
        }
        if (score < 79.9)
        {
            return "critical";
        }
        return "high";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveGarnishmentTier()
    {
        var policy = Retry.DefaultPolicy("adaptive-garnishment-tier");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
