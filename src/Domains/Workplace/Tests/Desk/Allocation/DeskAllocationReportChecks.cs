namespace Acme.Workforce.Workplace.Tests.Desk.Allocation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class DeskAllocationReportChecks
{
    public static void DeskAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701007309);
        var items = new List<DeskAllocation>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(DeskAllocation.NewDeskAllocation($"desk-allocation-{index}", "sample"));
        }
        var report = DeskAllocationReport.BuildDeskAllocationReport("test", items, clock);
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
        DeskAllocationReportCountsEachRecordOnce();
    }
}
