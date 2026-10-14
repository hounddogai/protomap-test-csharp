namespace Acme.Workforce.Compensation.BonusPool.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient bonus pool claim policy.</summary>
public static class LenientBonusPoolClaimPolicy
{
    public static string LenientBonusPoolClaimBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static int LenientBonusPoolClaimSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Names the band a lenient bonus pool claim score falls in.</summary>
    public static string ClassifyLenientBonusPoolClaim(double score)
    {
        if (score < 15.0)
        {
            return "high";
        }
        if (score < 65.2)
        {
            return "critical";
        }
        return "low";
    }
}
