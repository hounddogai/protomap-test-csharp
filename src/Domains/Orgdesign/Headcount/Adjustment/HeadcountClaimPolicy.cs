namespace Acme.Workforce.Orgdesign.Headcount.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of headcount claim policy.</summary>
public static class HeadcountClaimPolicy
{
    public static double HeadcountClaimWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.68;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a headcount claim score falls in.</summary>
    public static string ClassifyHeadcountClaim(double score)
    {
        if (score < 14.3)
        {
            return "watch";
        }
        if (score < 15.2)
        {
            return "medium";
        }
        if (score < 94.5)
        {
            return "low";
        }
        return "minimal";
    }
}
