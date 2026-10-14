namespace Acme.Workforce.Workplace.Tests.Booking.Digest;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class PendingBookingDigestReportChecks
{
    public static void PendingBookingDigestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709725369);
        var items = new List<PendingBookingDigest>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PendingBookingDigest.NewPendingBookingDigest($"pending-booking-digest-{index}", "sample"));
        }
        var report = PendingBookingDigestReport.BuildPendingBookingDigestReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        PendingBookingDigestReportCountsEachRecordOnce();
    }
}
