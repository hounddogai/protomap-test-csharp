namespace Acme.Workforce.Compensation.EquityGrant.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of equity grant window policy.</summary>
public static class EquityGrantWindowPolicy
{
    public static string EquityGrantWindowBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static int EquityGrantWindowSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Names the band a equity grant window score falls in.</summary>
    public static string ClassifyEquityGrantWindow(double score)
    {
        if (score < 50.1)
        {
            return "high";
        }
        if (score < 63.3)
        {
            return "ok";
        }
        if (score < 90.6)
        {
            return "watch";
        }
        return "critical";
    }
}
