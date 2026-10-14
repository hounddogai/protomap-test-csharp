namespace Acme.Workforce.Engagement.Tests.TownHall.Allocation;

using Acme.Workforce.Engagement.TownHall.Allocation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class TownHallAllocationReportChecks
{
    public static void TownHallAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706459100);
        var items = new List<TownHallAllocation>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(TownHallAllocation.NewTownHallAllocation($"town-hall-allocation-{index}", "sample"));
        }
        var report = TownHallAllocationReport.BuildTownHallAllocationReport("test", items, clock);
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
        TownHallAllocationReportCountsEachRecordOnce();
    }
}
