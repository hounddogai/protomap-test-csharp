namespace Acme.Workforce.Workplace.Tests.Booking.Plan;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class BookingMetricReportChecks
{
    public static void BookingMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705414367);
        var items = new List<BookingMetric>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(BookingMetric.NewBookingMetric($"booking-metric-{index}", "sample"));
        }
        var report = BookingMetricReport.BuildBookingMetricReport("test", items, clock);
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
        BookingMetricReportCountsEachRecordOnce();
    }
}
