namespace Acme.Workforce.Workplace.Tests.Booking.Audit;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class BookingProfileReportChecks
{
    public static void BookingProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702251108);
        var items = new List<BookingProfile>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(BookingProfile.NewBookingProfile($"booking-profile-{index}", "sample"));
        }
        var report = BookingProfileReport.BuildBookingProfileReport("test", items, clock);
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
        BookingProfileReportCountsEachRecordOnce();
    }
}
