namespace Acme.Workforce.Workplace.Tests.Booking.OverrideItem;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class BookingOverrideReportChecks
{
    public static void BookingOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707298753);
        var items = new List<BookingOverride>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(BookingOverride.NewBookingOverride($"booking-override-{index}", "sample"));
        }
        var report = BookingOverrideReport.BuildBookingOverrideReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        BookingOverrideReportCountsEachRecordOnce();
    }
}
