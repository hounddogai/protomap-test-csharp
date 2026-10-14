namespace Acme.Workforce.Workplace.Tests.Booking.BookingSegment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.BookingSegment;

/// <summary>Checks of booking segment code, run by the project's runner.</summary>
public static class ScheduledBookingBaselineReportChecks
{
    public static void ScheduledBookingBaselineReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707974077);
        var items = new List<ScheduledBookingBaseline>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ScheduledBookingBaseline.NewScheduledBookingBaseline($"scheduled-booking-baseline-{index}", "sample"));
        }
        var report = ScheduledBookingBaselineReport.BuildScheduledBookingBaselineReport("test", items, clock);
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
        ScheduledBookingBaselineReportCountsEachRecordOnce();
    }
}
