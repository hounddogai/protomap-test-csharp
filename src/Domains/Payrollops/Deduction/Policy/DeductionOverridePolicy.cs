namespace Acme.Workforce.Payrollops.Deduction.Policy;

/// <summary>Functions of deduction override policy.</summary>
public static class DeductionOverridePolicy
{
    public static int DeductionOverrideSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Names the band a deduction override score falls in.</summary>
    public static string ClassifyDeductionOverride(double score)
    {
        if (score < 15.3)
        {
            return "minimal";
        }
        if (score < 70.9)
        {
            return "high";
        }
        if (score < 90.8)
        {
            return "medium";
        }
        return "critical";
    }
}
