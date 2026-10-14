namespace Acme.Workforce.Talent.Pool.PoolWindow;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pool cycle policy.</summary>
public static class PoolCyclePolicy
{
    public static int PoolCycleSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Puts a pool cycle code in its canonical PC- form.</summary>
    public static string NormalizePoolCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePoolCycle()
    {
        var policy = Retry.DefaultPolicy("pool-cycle");
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
