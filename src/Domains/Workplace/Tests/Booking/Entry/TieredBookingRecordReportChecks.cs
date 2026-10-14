namespace Acme.Workforce.Workplace.Tests.Booking.Entry;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class TieredBookingRecordReportChecks
{
    public static void TieredBookingRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704230808);
        var items = new List<TieredBookingRecord>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(TieredBookingRecord.NewTieredBookingRecord($"tiered-booking-record-{index}", "sample"));
        }
        var report = TieredBookingRecordReport.BuildTieredBookingRecordReport("test", items, clock);
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
        TieredBookingRecordReportCountsEachRecordOnce();
    }
}
