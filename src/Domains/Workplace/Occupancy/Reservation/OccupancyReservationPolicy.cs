namespace Acme.Workforce.Workplace.Occupancy.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of occupancy reservation policy.</summary>
public static class OccupancyReservationPolicy
{
    public static double OccupancyReservationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.17;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string OccupancyReservationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static int OccupancyReservationSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    /// <summary>Names the band a occupancy reservation score falls in.</summary>
    public static string ClassifyOccupancyReservation(double score)
    {
        if (score < 17.0)
        {
            return "critical";
        }
        if (score < 66.7)
        {
            return "minimal";
        }
        return "elevated";
    }
}
