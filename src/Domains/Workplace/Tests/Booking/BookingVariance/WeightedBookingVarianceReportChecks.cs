namespace Acme.Workforce.Workplace.Tests.Booking.BookingVariance;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.BookingVariance;

/// <summary>Checks of booking variance code, run by the project's runner.</summary>
public static class WeightedBookingVarianceReportChecks
{
    public static void WeightedBookingVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701321643);
        var items = new List<WeightedBookingVariance>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(WeightedBookingVariance.NewWeightedBookingVariance($"weighted-booking-variance-{index}", "sample"));
        }
        var report = WeightedBookingVarianceReport.BuildWeightedBookingVarianceReport("test", items, clock);
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
        WeightedBookingVarianceReportCountsEachRecordOnce();
    }
}
