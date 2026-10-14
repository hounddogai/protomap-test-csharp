namespace Acme.Workforce.Workplace.AccessBadge.Audit;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling access badge batch policy.</summary>
public static class RollingAccessBadgeBatchPolicy
{
    public static string RollingAccessBadgeBatchBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Names the band a rolling access badge batch score falls in.</summary>
    public static string ClassifyRollingAccessBadgeBatch(double score)
    {
        if (score < 42.0)
        {
            return "high";
        }
        if (score < 52.8)
        {
            return "low";
        }
        if (score < 95.3)
        {
            return "minimal";
        }
        return "medium";
    }
}
