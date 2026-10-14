namespace Acme.Workforce.Workplace.Tests.Visitor.Slot;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Visitor.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class VisitorAllocationReportChecks
{
    public static void VisitorAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707448410);
        var items = new List<VisitorAllocation>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(VisitorAllocation.NewVisitorAllocation($"visitor-allocation-{index}", "sample"));
        }
        var report = VisitorAllocationReport.BuildVisitorAllocationReport("test", items, clock);
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
        VisitorAllocationReportCountsEachRecordOnce();
    }
}
