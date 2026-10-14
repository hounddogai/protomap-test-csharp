namespace Acme.Workforce.Talent.Pool.Plan;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of capped pool audit policy.</summary>
public static class CappedPoolAuditPolicy
{
    public static int CappedPoolAuditSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    public static double CappedPoolAuditWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.49;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedPoolAudit()
    {
        var policy = Retry.DefaultPolicy("capped-pool-audit");
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

    /// <summary>Puts a capped pool audit code in its canonical CPA- form.</summary>
    public static string NormalizeCappedPoolAuditCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CPA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CPA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
