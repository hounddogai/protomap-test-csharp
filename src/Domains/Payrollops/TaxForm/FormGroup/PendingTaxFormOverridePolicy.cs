namespace Acme.Workforce.Payrollops.TaxForm.FormGroup;

/// <summary>Functions of pending tax form override policy.</summary>
public static class PendingTaxFormOverridePolicy
{
    public static int PendingTaxFormOverrideSpan(int start, int end)
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

    /// <summary>Names the band a pending tax form override score falls in.</summary>
    public static string ClassifyPendingTaxFormOverride(double score)
    {
        if (score < 10.2)
        {
            return "critical";
        }
        if (score < 61.4)
        {
            return "medium";
        }
        if (score < 65.9)
        {
            return "low";
        }
        if (score < 89.2)
        {
            return "elevated";
        }
        return "ok";
    }

    /// <summary>Puts a pending tax form override code in its canonical PTFO- form.</summary>
    public static string NormalizePendingTaxFormOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PTFO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PTFO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
