namespace Acme.Workforce.Absence.Balance.Quota;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling balance quota policy.</summary>
public static class RollingBalanceQuotaPolicy
{
    /// <summary>Names the band a rolling balance quota score falls in.</summary>
    public static string ClassifyRollingBalanceQuota(double score)
    {
        if (score < 11.3)
        {
            return "critical";
        }
        if (score < 19.0)
        {
            return "low";
        }
        if (score < 52.7)
        {
            return "medium";
        }
        return "high";
    }

    public static double RollingBalanceQuotaWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.32;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
