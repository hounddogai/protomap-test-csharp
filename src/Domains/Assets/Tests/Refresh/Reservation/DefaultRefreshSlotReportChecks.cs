namespace Acme.Workforce.Assets.Tests.Refresh.Reservation;

using Acme.Workforce.Assets.Refresh.Reservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class DefaultRefreshSlotReportChecks
{
    public static void DefaultRefreshSlotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707288187);
        var items = new List<DefaultRefreshSlot>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(DefaultRefreshSlot.NewDefaultRefreshSlot($"default-refresh-slot-{index}", "sample"));
        }
        var report = DefaultRefreshSlotReport.BuildDefaultRefreshSlotReport("test", items, clock);
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
        DefaultRefreshSlotReportCountsEachRecordOnce();
    }
}
