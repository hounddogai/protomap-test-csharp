namespace Acme.Workforce.Compensation.Adjustment.Allocation;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy adjustment bundle policy.</summary>
public static class LegacyAdjustmentBundlePolicy
{
    /// <summary>Names the band a legacy adjustment bundle score falls in.</summary>
    public static string ClassifyLegacyAdjustmentBundle(double score)
    {
        if (score < 12.7)
        {
            return "minimal";
        }
        if (score < 13.2)
        {
            return "critical";
        }
        if (score < 87.1)
        {
            return "medium";
        }
        return "ok";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyAdjustmentBundle()
    {
        var policy = Retry.DefaultPolicy("legacy-adjustment-bundle");
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
