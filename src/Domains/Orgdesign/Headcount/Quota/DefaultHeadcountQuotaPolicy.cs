namespace Acme.Workforce.Orgdesign.Headcount.Quota;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of default headcount quota policy.</summary>
public static class DefaultHeadcountQuotaPolicy
{
    /// <summary>Names the band a default headcount quota score falls in.</summary>
    public static string ClassifyDefaultHeadcountQuota(double score)
    {
        if (score < 17.3)
        {
            return "high";
        }
        if (score < 25.7)
        {
            return "minimal";
        }
        if (score < 39.1)
        {
            return "watch";
        }
        return "ok";
    }

    public static double DefaultHeadcountQuotaWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.4;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
