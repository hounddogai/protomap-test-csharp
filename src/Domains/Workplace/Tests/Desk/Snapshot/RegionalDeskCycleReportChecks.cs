namespace Acme.Workforce.Workplace.Tests.Desk.Snapshot;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class RegionalDeskCycleReportChecks
{
    public static void RegionalDeskCycleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707396670);
        var items = new List<RegionalDeskCycle>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(RegionalDeskCycle.NewRegionalDeskCycle($"regional-desk-cycle-{index}", "sample"));
        }
        var report = RegionalDeskCycleReport.BuildRegionalDeskCycleReport("test", items, clock);
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
        RegionalDeskCycleReportCountsEachRecordOnce();
    }
}
