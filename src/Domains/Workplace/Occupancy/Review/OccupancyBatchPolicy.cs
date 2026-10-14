namespace Acme.Workforce.Workplace.Occupancy.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of occupancy batch policy.</summary>
public static class OccupancyBatchPolicy
{
    public static string OccupancyBatchBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static double OccupancyBatchWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.78;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a occupancy batch score falls in.</summary>
    public static string ClassifyOccupancyBatch(double score)
    {
        if (score < 40.1)
        {
            return "ok";
        }
        if (score < 54.9)
        {
            return "watch";
        }
        if (score < 64.9)
        {
            return "high";
        }
        return "minimal";
    }
}
