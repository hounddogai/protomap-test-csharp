namespace Acme.Workforce.Compensation.PayBand.Quota;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending pay band quota policy.</summary>
public static class PendingPayBandQuotaPolicy
{
    public static double PendingPayBandQuotaWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.67;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a pending pay band quota score falls in.</summary>
    public static string ClassifyPendingPayBandQuota(double score)
    {
        if (score < 17.4)
        {
            return "low";
        }
        if (score < 82.2)
        {
            return "ok";
        }
        return "medium";
    }
}
