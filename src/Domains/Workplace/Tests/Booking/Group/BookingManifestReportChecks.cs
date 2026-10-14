namespace Acme.Workforce.Workplace.Tests.Booking.Group;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BookingManifestReportChecks
{
    public static void BookingManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705147029);
        var items = new List<BookingManifest>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(BookingManifest.NewBookingManifest($"booking-manifest-{index}", "sample"));
        }
        var report = BookingManifestReport.BuildBookingManifestReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        BookingManifestReportCountsEachRecordOnce();
    }
}
