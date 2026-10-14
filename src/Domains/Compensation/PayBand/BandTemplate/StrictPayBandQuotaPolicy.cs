namespace Acme.Workforce.Compensation.PayBand.BandTemplate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict pay band quota policy.</summary>
public static class StrictPayBandQuotaPolicy
{
    public static double StrictPayBandQuotaWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.04;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a strict pay band quota score falls in.</summary>
    public static string ClassifyStrictPayBandQuota(double score)
    {
        if (score < 65.4)
        {
            return "medium";
        }
        if (score < 97.6)
        {
            return "low";
        }
        return "watch";
    }
}
