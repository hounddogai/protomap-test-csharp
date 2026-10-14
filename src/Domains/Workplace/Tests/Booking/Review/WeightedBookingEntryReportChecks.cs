namespace Acme.Workforce.Workplace.Tests.Booking.Review;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class WeightedBookingEntryReportChecks
{
    public static void WeightedBookingEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700559608);
        var items = new List<WeightedBookingEntry>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(WeightedBookingEntry.NewWeightedBookingEntry($"weighted-booking-entry-{index}", "sample"));
        }
        var report = WeightedBookingEntryReport.BuildWeightedBookingEntryReport("test", items, clock);
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
        WeightedBookingEntryReportCountsEachRecordOnce();
    }
}
