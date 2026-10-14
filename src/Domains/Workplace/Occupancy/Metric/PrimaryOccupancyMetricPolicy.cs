namespace Acme.Workforce.Workplace.Occupancy.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary occupancy metric policy.</summary>
public static class PrimaryOccupancyMetricPolicy
{
    /// <summary>Names the band a primary occupancy metric score falls in.</summary>
    public static string ClassifyPrimaryOccupancyMetric(double score)
    {
        if (score < 11.0)
        {
            return "watch";
        }
        if (score < 73.6)
        {
            return "elevated";
        }
        if (score < 86.3)
        {
            return "low";
        }
        return "minimal";
    }

    public static string PrimaryOccupancyMetricBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
