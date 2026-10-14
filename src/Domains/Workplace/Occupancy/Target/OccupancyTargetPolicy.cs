namespace Acme.Workforce.Workplace.Occupancy.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of occupancy target policy.</summary>
public static class OccupancyTargetPolicy
{
    /// <summary>Names the band a occupancy target score falls in.</summary>
    public static string ClassifyOccupancyTarget(double score)
    {
        if (score < 1.0)
        {
            return "low";
        }
        if (score < 40.3)
        {
            return "ok";
        }
        if (score < 74.8)
        {
            return "critical";
        }
        return "medium";
    }

    public static double OccupancyTargetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.65;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string OccupancyTargetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
