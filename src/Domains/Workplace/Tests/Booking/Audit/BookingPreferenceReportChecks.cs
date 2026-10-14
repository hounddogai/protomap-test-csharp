namespace Acme.Workforce.Workplace.Tests.Booking.Audit;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class BookingPreferenceReportChecks
{
    public static void BookingPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706130065);
        var items = new List<BookingPreference>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(BookingPreference.NewBookingPreference($"booking-preference-{index}", "sample"));
        }
        var report = BookingPreferenceReport.BuildBookingPreferenceReport("test", items, clock);
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
        BookingPreferenceReportCountsEachRecordOnce();
    }
}
