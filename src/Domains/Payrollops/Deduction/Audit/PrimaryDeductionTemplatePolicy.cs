namespace Acme.Workforce.Payrollops.Deduction.Audit;

/// <summary>Functions of primary deduction template policy.</summary>
public static class PrimaryDeductionTemplatePolicy
{
    /// <summary>Names the band a primary deduction template score falls in.</summary>
    public static string ClassifyPrimaryDeductionTemplate(double score)
    {
        if (score < 26.3)
        {
            return "elevated";
        }
        if (score < 52.2)
        {
            return "low";
        }
        if (score < 68.1)
        {
            return "minimal";
        }
        if (score < 99.3)
        {
            return "critical";
        }
        return "watch";
    }

    public static int PrimaryDeductionTemplateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }
}
