namespace Acme.Workforce.Absence.Balance.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of balance rollup policy.</summary>
public static class BalanceRollupPolicy
{
    public static string BalanceRollupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a balance rollup score falls in.</summary>
    public static string ClassifyBalanceRollup(double score)
    {
        if (score < 23.4)
        {
            return "ok";
        }
        if (score < 63.6)
        {
            return "minimal";
        }
        if (score < 69.8)
        {
            return "critical";
        }
        if (score < 99.6)
        {
            return "high";
        }
        return "low";
    }
}
