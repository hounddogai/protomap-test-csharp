namespace Acme.Workforce.Workplace.Tests.Booking.BookingAudit;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.BookingAudit;

/// <summary>Checks of booking audit code, run by the project's runner.</summary>
public static class BookingSnapshotReportChecks
{
    public static void BookingSnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705235705);
        var items = new List<BookingSnapshot>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(BookingSnapshot.NewBookingSnapshot($"booking-snapshot-{index}", "sample"));
        }
        var report = BookingSnapshotReport.BuildBookingSnapshotReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        BookingSnapshotReportCountsEachRecordOnce();
    }
}
