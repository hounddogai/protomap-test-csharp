namespace Acme.Workforce.Talent.SuccessionPlan.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict succession plan claim policy.</summary>
public static class StrictSuccessionPlanClaimPolicy
{
    public static string StrictSuccessionPlanClaimBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static int StrictSuccessionPlanClaimSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Names the band a strict succession plan claim score falls in.</summary>
    public static string ClassifyStrictSuccessionPlanClaim(double score)
    {
        if (score < 24.9)
        {
            return "low";
        }
        if (score < 79.5)
        {
            return "watch";
        }
        return "high";
    }
}
