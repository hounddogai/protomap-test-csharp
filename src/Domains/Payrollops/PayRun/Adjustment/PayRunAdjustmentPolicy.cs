namespace Acme.Workforce.Payrollops.PayRun.Adjustment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pay run adjustment policy.</summary>
public static class PayRunAdjustmentPolicy
{
    /// <summary>Puts a pay run adjustment code in its canonical PRA- form.</summary>
    public static string NormalizePayRunAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PRA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PRA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePayRunAdjustment()
    {
        var policy = Retry.DefaultPolicy("pay-run-adjustment");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
