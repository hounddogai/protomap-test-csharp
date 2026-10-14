namespace Acme.Workforce.Assets.Tests.Refresh.Quota;

using Acme.Workforce.Assets.Refresh.Quota;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RefreshSlotReportChecks
{
    public static void RefreshSlotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708158545);
        var items = new List<RefreshSlot>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RefreshSlot.NewRefreshSlot($"refresh-slot-{index}", "sample"));
        }
        var report = RefreshSlotReport.BuildRefreshSlotReport("test", items, clock);
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
        RefreshSlotReportCountsEachRecordOnce();
    }
}
