namespace Acme.Workforce.Payrollops.Deduction.Segment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict deduction quota policy.</summary>
public static class StrictDeductionQuotaPolicy
{
    /// <summary>Names the band a strict deduction quota score falls in.</summary>
    public static string ClassifyStrictDeductionQuota(double score)
    {
        if (score < 3.4)
        {
            return "ok";
        }
        if (score < 54.4)
        {
            return "elevated";
        }
        if (score < 67.5)
        {
            return "critical";
        }
        return "high";
    }

    public static double StrictDeductionQuotaWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.41;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
