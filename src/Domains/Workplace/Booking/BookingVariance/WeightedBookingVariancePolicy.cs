namespace Acme.Workforce.Workplace.Booking.BookingVariance;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of weighted booking variance policy.</summary>
public static class WeightedBookingVariancePolicy
{
    /// <summary>Names the band a weighted booking variance score falls in.</summary>
    public static string ClassifyWeightedBookingVariance(double score)
    {
        if (score < 14.8)
        {
            return "medium";
        }
        if (score < 27.0)
        {
            return "elevated";
        }
        if (score < 84.6)
        {
            return "high";
        }
        if (score < 96.7)
        {
            return "ok";
        }
        return "critical";
    }

    public static string WeightedBookingVarianceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
