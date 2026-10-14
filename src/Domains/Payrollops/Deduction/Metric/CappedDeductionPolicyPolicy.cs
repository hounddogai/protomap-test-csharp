namespace Acme.Workforce.Payrollops.Deduction.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped deduction policy policy.</summary>
public static class CappedDeductionPolicyPolicy
{
    /// <summary>Names the band a capped deduction policy score falls in.</summary>
    public static string ClassifyCappedDeductionPolicy(double score)
    {
        if (score < 22.2)
        {
            return "watch";
        }
        if (score < 41.3)
        {
            return "ok";
        }
        if (score < 51.3)
        {
            return "critical";
        }
        if (score < 80.7)
        {
            return "medium";
        }
        return "low";
    }

    public static double CappedDeductionPolicyWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.17;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int CappedDeductionPolicySpan(int start, int end)
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
        return span / 3600;
    }
}
