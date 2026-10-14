namespace Acme.Workforce.Workplace.Booking.BookingVariance;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy booking audit policy.</summary>
public static class LegacyBookingAuditPolicy
{
    public static int LegacyBookingAuditSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a legacy booking audit score falls in.</summary>
    public static string ClassifyLegacyBookingAudit(double score)
    {
        if (score < 24.1)
        {
            return "minimal";
        }
        if (score < 25.3)
        {
            return "medium";
        }
        if (score < 57.9)
        {
            return "elevated";
        }
        if (score < 91.5)
        {
            return "high";
        }
        return "ok";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyBookingAudit()
    {
        var policy = Retry.DefaultPolicy("legacy-booking-audit");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static double LegacyBookingAuditWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.14;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
