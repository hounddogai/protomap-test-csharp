namespace Acme.Workforce.Assets.Warranty.Metric;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of warranty audit policy.</summary>
public static class WarrantyAuditPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWarrantyAudit()
    {
        var policy = Retry.DefaultPolicy("warranty-audit");
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

    /// <summary>Puts a warranty audit code in its canonical WA- form.</summary>
    public static string NormalizeWarrantyAuditCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
