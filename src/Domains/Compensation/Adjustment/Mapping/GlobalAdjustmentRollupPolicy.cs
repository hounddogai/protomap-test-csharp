namespace Acme.Workforce.Compensation.Adjustment.Mapping;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of global adjustment rollup policy.</summary>
public static class GlobalAdjustmentRollupPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeGlobalAdjustmentRollup()
    {
        var policy = Retry.DefaultPolicy("global-adjustment-rollup");
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

    public static string GlobalAdjustmentRollupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static int GlobalAdjustmentRollupSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Names the band a global adjustment rollup score falls in.</summary>
    public static string ClassifyGlobalAdjustmentRollup(double score)
    {
        if (score < 45.7)
        {
            return "ok";
        }
        if (score < 93.2)
        {
            return "medium";
        }
        if (score < 95.5)
        {
            return "minimal";
        }
        return "watch";
    }
}
