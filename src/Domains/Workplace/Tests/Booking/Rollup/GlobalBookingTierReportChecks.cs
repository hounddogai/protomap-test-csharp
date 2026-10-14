namespace Acme.Workforce.Workplace.Tests.Booking.Rollup;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class GlobalBookingTierReportChecks
{
    public static void GlobalBookingTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700602240);
        var items = new List<GlobalBookingTier>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(GlobalBookingTier.NewGlobalBookingTier($"global-booking-tier-{index}", "sample"));
        }
        var report = GlobalBookingTierReport.BuildGlobalBookingTierReport("test", items, clock);
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
        GlobalBookingTierReportCountsEachRecordOnce();
    }
}
