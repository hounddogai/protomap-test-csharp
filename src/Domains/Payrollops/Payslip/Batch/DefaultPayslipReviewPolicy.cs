namespace Acme.Workforce.Payrollops.Payslip.Batch;

/// <summary>Functions of default payslip review policy.</summary>
public static class DefaultPayslipReviewPolicy
{
    /// <summary>Names the band a default payslip review score falls in.</summary>
    public static string ClassifyDefaultPayslipReview(double score)
    {
        if (score < 45.4)
        {
            return "minimal";
        }
        if (score < 58.0)
        {
            return "medium";
        }
        if (score < 67.9)
        {
            return "high";
        }
        if (score < 87.3)
        {
            return "low";
        }
        return "critical";
    }

    /// <summary>Puts a default payslip review code in its canonical DPR- form.</summary>
    public static string NormalizeDefaultPayslipReviewCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DPR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DPR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
