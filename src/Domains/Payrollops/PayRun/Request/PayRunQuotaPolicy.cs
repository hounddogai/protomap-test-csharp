namespace Acme.Workforce.Payrollops.PayRun.Request;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pay run quota policy.</summary>
public static class PayRunQuotaPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePayRunQuota()
    {
        var policy = Retry.DefaultPolicy("pay-run-quota");
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

    /// <summary>Puts a pay run quota code in its canonical PRQ- form.</summary>
    public static string NormalizePayRunQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PRQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PRQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
