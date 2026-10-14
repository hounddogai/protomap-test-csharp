namespace Acme.Workforce.Payrollops.Deduction.DeductionAudit;

/// <summary>Functions of adaptive deduction audit policy.</summary>
public static class AdaptiveDeductionAuditPolicy
{
    /// <summary>Puts a adaptive deduction audit code in its canonical ADA- form.</summary>
    public static string NormalizeAdaptiveDeductionAuditCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ADA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ADA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int AdaptiveDeductionAuditSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }
}
