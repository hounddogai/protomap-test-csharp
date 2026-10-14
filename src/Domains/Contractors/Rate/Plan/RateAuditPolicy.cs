namespace Acme.Workforce.Contractors.Rate.Plan;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of rate audit policy.</summary>
public static class RateAuditPolicy
{
    public static int RateAuditSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Puts a rate audit code in its canonical RA- form.</summary>
    public static string NormalizeRateAuditCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRateAudit()
    {
        var policy = Retry.DefaultPolicy("rate-audit");
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
