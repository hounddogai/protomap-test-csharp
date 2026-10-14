namespace Acme.Workforce.Talent.SuccessionPlan.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional succession plan allocation policy.</summary>
public static class RegionalSuccessionPlanAllocationPolicy
{
    /// <summary>Names the band a regional succession plan allocation score falls in.</summary>
    public static string ClassifyRegionalSuccessionPlanAllocation(double score)
    {
        if (score < 44.3)
        {
            return "ok";
        }
        if (score < 50.9)
        {
            return "low";
        }
        if (score < 63.3)
        {
            return "critical";
        }
        return "medium";
    }

    public static double RegionalSuccessionPlanAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.27;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int RegionalSuccessionPlanAllocationSpan(int start, int end)
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
}
