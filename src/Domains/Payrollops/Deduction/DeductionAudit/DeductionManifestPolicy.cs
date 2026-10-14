namespace Acme.Workforce.Payrollops.Deduction.DeductionAudit;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of deduction manifest policy.</summary>
public static class DeductionManifestPolicy
{
    /// <summary>Names the band a deduction manifest score falls in.</summary>
    public static string ClassifyDeductionManifest(double score)
    {
        if (score < 10.6)
        {
            return "high";
        }
        if (score < 20.3)
        {
            return "critical";
        }
        if (score < 44.7)
        {
            return "elevated";
        }
        return "minimal";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDeductionManifest()
    {
        var policy = Retry.DefaultPolicy("deduction-manifest");
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

    /// <summary>Puts a deduction manifest code in its canonical DM- form.</summary>
    public static string NormalizeDeductionManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
