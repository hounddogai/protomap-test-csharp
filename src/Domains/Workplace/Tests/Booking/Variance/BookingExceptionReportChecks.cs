namespace Acme.Workforce.Workplace.Tests.Booking.Variance;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class BookingExceptionReportChecks
{
    public static void BookingExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703339483);
        var items = new List<BookingException>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(BookingException.NewBookingException($"booking-exception-{index}", "sample"));
        }
        var report = BookingExceptionReport.BuildBookingExceptionReport("test", items, clock);
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
        BookingExceptionReportCountsEachRecordOnce();
    }
}
