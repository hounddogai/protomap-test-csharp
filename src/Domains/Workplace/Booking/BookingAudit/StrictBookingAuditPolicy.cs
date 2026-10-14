namespace Acme.Workforce.Workplace.Booking.BookingAudit;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict booking audit policy.</summary>
public static class StrictBookingAuditPolicy
{
    /// <summary>Names the band a strict booking audit score falls in.</summary>
    public static string ClassifyStrictBookingAudit(double score)
    {
        if (score < 14.1)
        {
            return "high";
        }
        if (score < 36.9)
        {
            return "low";
        }
        if (score < 65.0)
        {
            return "critical";
        }
        if (score < 67.1)
        {
            return "elevated";
        }
        return "minimal";
    }

    public static double StrictBookingAuditWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.71;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int StrictBookingAuditSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }
}
