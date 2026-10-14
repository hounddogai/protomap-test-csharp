namespace Acme.Workforce.Workplace.Occupancy.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of occupancy review policy.</summary>
public static class OccupancyReviewPolicy
{
    /// <summary>Names the band a occupancy review score falls in.</summary>
    public static string ClassifyOccupancyReview(double score)
    {
        if (score < 21.2)
        {
            return "watch";
        }
        if (score < 32.9)
        {
            return "minimal";
        }
        return "ok";
    }

    public static int OccupancyReviewSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    public static string OccupancyReviewBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
