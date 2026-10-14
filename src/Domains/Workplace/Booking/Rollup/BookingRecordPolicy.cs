namespace Acme.Workforce.Workplace.Booking.Rollup;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of booking record policy.</summary>
public static class BookingRecordPolicy
{
    public static int BookingRecordSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBookingRecord()
    {
        var policy = Retry.DefaultPolicy("booking-record");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a booking record score falls in.</summary>
    public static string ClassifyBookingRecord(double score)
    {
        if (score < 35.1)
        {
            return "critical";
        }
        if (score < 79.6)
        {
            return "elevated";
        }
        if (score < 85.0)
        {
            return "low";
        }
        return "high";
    }
}
