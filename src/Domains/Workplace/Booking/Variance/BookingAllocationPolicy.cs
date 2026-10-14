namespace Acme.Workforce.Workplace.Booking.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of booking allocation policy.</summary>
public static class BookingAllocationPolicy
{
    public static double BookingAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.39;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string BookingAllocationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
