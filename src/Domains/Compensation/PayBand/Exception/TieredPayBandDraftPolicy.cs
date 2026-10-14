namespace Acme.Workforce.Compensation.PayBand.Exception;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered pay band draft policy.</summary>
public static class TieredPayBandDraftPolicy
{
    public static double TieredPayBandDraftWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.08;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a tiered pay band draft score falls in.</summary>
    public static string ClassifyTieredPayBandDraft(double score)
    {
        if (score < 16.8)
        {
            return "elevated";
        }
        if (score < 33.7)
        {
            return "medium";
        }
        if (score < 56.4)
        {
            return "high";
        }
        if (score < 91.0)
        {
            return "low";
        }
        return "watch";
    }
}
