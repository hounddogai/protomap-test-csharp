namespace Acme.Workforce.Absence.Holiday.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of holiday reservation policy.</summary>
public static class HolidayReservationPolicy
{
    public static int HolidayReservationSpan(int start, int end)
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
        return span / 3600;
    }

    public static double HolidayReservationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.13;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
