namespace Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pending bonus pool policy policy.</summary>
public static class PendingBonusPoolPolicyPolicy
{
    /// <summary>Puts a pending bonus pool policy code in its canonical PBPP- form.</summary>
    public static string NormalizePendingBonusPoolPolicyCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PBPP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PBPP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingBonusPoolPolicy()
    {
        var policy = Retry.DefaultPolicy("pending-bonus-pool-policy");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static int PendingBonusPoolPolicySpan(int start, int end)
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
        return span / 3600;
    }
}
