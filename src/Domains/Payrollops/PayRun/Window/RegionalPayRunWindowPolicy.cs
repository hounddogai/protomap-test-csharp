namespace Acme.Workforce.Payrollops.PayRun.Window;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of regional pay run window policy.</summary>
public static class RegionalPayRunWindowPolicy
{
    /// <summary>Puts a regional pay run window code in its canonical RPRW- form.</summary>
    public static string NormalizeRegionalPayRunWindowCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPRW-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPRW-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRegionalPayRunWindow()
    {
        var policy = Retry.DefaultPolicy("regional-pay-run-window");
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
