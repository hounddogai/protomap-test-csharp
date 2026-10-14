namespace Acme.Workforce.Workplace.Booking.Assignment;

/// <summary>Functions of pending booking audit policy.</summary>
public static class PendingBookingAuditPolicy
{
    /// <summary>Names the band a pending booking audit score falls in.</summary>
    public static string ClassifyPendingBookingAudit(double score)
    {
        if (score < 5.0)
        {
            return "medium";
        }
        if (score < 69.1)
        {
            return "ok";
        }
        if (score < 87.2)
        {
            return "low";
        }
        return "critical";
    }

    public static int PendingBookingAuditSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }
}
