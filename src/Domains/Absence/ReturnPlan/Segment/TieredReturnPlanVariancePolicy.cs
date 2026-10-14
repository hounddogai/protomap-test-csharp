namespace Acme.Workforce.Absence.ReturnPlan.Segment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered return plan variance policy.</summary>
public static class TieredReturnPlanVariancePolicy
{
    /// <summary>Names the band a tiered return plan variance score falls in.</summary>
    public static string ClassifyTieredReturnPlanVariance(double score)
    {
        if (score < 1.0)
        {
            return "elevated";
        }
        if (score < 28.1)
        {
            return "watch";
        }
        if (score < 29.3)
        {
            return "low";
        }
        if (score < 44.9)
        {
            return "critical";
        }
        return "minimal";
    }

    public static double TieredReturnPlanVarianceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.25;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string TieredReturnPlanVarianceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
