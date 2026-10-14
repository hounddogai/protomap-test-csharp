namespace Acme.Workforce.Compensation.MeritCycle.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback merit cycle claim policy.</summary>
public static class FallbackMeritCycleClaimPolicy
{
    public static double FallbackMeritCycleClaimWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.79;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a fallback merit cycle claim score falls in.</summary>
    public static string ClassifyFallbackMeritCycleClaim(double score)
    {
        if (score < 5.0)
        {
            return "watch";
        }
        if (score < 84.0)
        {
            return "elevated";
        }
        if (score < 89.0)
        {
            return "medium";
        }
        return "low";
    }

    public static string FallbackMeritCycleClaimBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
