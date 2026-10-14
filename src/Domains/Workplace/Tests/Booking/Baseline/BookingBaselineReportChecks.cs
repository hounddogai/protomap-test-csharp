namespace Acme.Workforce.Workplace.Tests.Booking.Baseline;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class BookingBaselineReportChecks
{
    public static void BookingBaselineReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708304121);
        var items = new List<BookingBaseline>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(BookingBaseline.NewBookingBaseline($"booking-baseline-{index}", "sample"));
        }
        var report = BookingBaselineReport.BuildBookingBaselineReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        BookingBaselineReportCountsEachRecordOnce();
    }
}
