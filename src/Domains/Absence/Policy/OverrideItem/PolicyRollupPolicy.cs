namespace Acme.Workforce.Absence.Policy.OverrideItem;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of policy rollup policy.</summary>
public static class PolicyRollupPolicy
{
    public static int PolicyRollupSpan(int start, int end)
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
    public static int ProbePolicyRollup()
    {
        var policy = Retry.DefaultPolicy("policy-rollup");
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
}
