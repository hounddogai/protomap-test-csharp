namespace Acme.Workforce.Talent.Pool.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pool schedule policy.</summary>
public static class PoolSchedulePolicy
{
    /// <summary>Names the band a pool schedule score falls in.</summary>
    public static string ClassifyPoolSchedule(double score)
    {
        if (score < 86.6)
        {
            return "high";
        }
        if (score < 87.6)
        {
            return "critical";
        }
        if (score < 90.7)
        {
            return "ok";
        }
        return "watch";
    }

    public static string PoolScheduleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
