namespace Acme.Workforce.Talent.SuccessionPlan.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered succession plan claim policy.</summary>
public static class TieredSuccessionPlanClaimPolicy
{
    public static double TieredSuccessionPlanClaimWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.46;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a tiered succession plan claim score falls in.</summary>
    public static string ClassifyTieredSuccessionPlanClaim(double score)
    {
        if (score < 83.7)
        {
            return "low";
        }
        if (score < 85.4)
        {
            return "high";
        }
        return "elevated";
    }

    public static int TieredSuccessionPlanClaimSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }
}
