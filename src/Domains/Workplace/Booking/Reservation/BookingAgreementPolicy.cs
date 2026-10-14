namespace Acme.Workforce.Workplace.Booking.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of booking agreement policy.</summary>
public static class BookingAgreementPolicy
{
    public static int BookingAgreementSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }

    public static double BookingAgreementWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.38;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
