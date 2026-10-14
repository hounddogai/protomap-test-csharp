namespace Acme.Workforce.Absence.ReturnPlan.OverrideItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback return plan quota policy.</summary>
public static class FallbackReturnPlanQuotaPolicy
{
    /// <summary>Names the band a fallback return plan quota score falls in.</summary>
    public static string ClassifyFallbackReturnPlanQuota(double score)
    {
        if (score < 13.3)
        {
            return "critical";
        }
        if (score < 24.8)
        {
            return "watch";
        }
        if (score < 89.8)
        {
            return "low";
        }
        return "ok";
    }

    public static double FallbackReturnPlanQuotaWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.78;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
