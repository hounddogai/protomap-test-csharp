namespace Acme.Workforce.Talent.Pool.Quota;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pool quota policy.</summary>
public static class PoolQuotaPolicy
{
    /// <summary>Names the band a pool quota score falls in.</summary>
    public static string ClassifyPoolQuota(double score)
    {
        if (score < 41.6)
        {
            return "watch";
        }
        if (score < 61.7)
        {
            return "minimal";
        }
        if (score < 87.5)
        {
            return "medium";
        }
        return "ok";
    }

    public static string PoolQuotaBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
