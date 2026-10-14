namespace Acme.Workforce.Assets.Refresh.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending refresh mapping policy.</summary>
public static class PendingRefreshMappingPolicy
{
    public static double PendingRefreshMappingWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.99;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string PendingRefreshMappingBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Names the band a pending refresh mapping score falls in.</summary>
    public static string ClassifyPendingRefreshMapping(double score)
    {
        if (score < 11.3)
        {
            return "low";
        }
        if (score < 40.6)
        {
            return "critical";
        }
        if (score < 71.7)
        {
            return "minimal";
        }
        return "high";
    }
}
