namespace Acme.Workforce.Payrollops.Deduction.Exception;

/// <summary>Functions of weighted deduction plan policy.</summary>
public static class WeightedDeductionPlanPolicy
{
    public static int WeightedDeductionPlanSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Names the band a weighted deduction plan score falls in.</summary>
    public static string ClassifyWeightedDeductionPlan(double score)
    {
        if (score < 31.6)
        {
            return "high";
        }
        if (score < 52.0)
        {
            return "low";
        }
        return "watch";
    }
}
