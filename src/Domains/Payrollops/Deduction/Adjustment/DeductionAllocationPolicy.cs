namespace Acme.Workforce.Payrollops.Deduction.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of deduction allocation policy.</summary>
public static class DeductionAllocationPolicy
{
    public static string DeductionAllocationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static int DeductionAllocationSpan(int start, int end)
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
        return span / 86400;
    }

    public static double DeductionAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.71;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a deduction allocation score falls in.</summary>
    public static string ClassifyDeductionAllocation(double score)
    {
        if (score < 21.0)
        {
            return "medium";
        }
        if (score < 28.1)
        {
            return "high";
        }
        if (score < 85.2)
        {
            return "watch";
        }
        if (score < 87.1)
        {
            return "ok";
        }
        return "low";
    }
}
