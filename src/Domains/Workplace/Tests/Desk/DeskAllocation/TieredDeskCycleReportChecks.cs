namespace Acme.Workforce.Workplace.Tests.Desk.DeskAllocation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.DeskAllocation;

/// <summary>Checks of desk allocation code, run by the project's runner.</summary>
public static class TieredDeskCycleReportChecks
{
    public static void TieredDeskCycleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702727332);
        var items = new List<TieredDeskCycle>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(TieredDeskCycle.NewTieredDeskCycle($"tiered-desk-cycle-{index}", "sample"));
        }
        var report = TieredDeskCycleReport.BuildTieredDeskCycleReport("test", items, clock);
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
        TieredDeskCycleReportCountsEachRecordOnce();
    }
}
