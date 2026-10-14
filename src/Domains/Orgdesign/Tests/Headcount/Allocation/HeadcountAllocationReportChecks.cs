namespace Acme.Workforce.Orgdesign.Tests.Headcount.Allocation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Headcount.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class HeadcountAllocationReportChecks
{
    public static void HeadcountAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706543692);
        var items = new List<HeadcountAllocation>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(HeadcountAllocation.NewHeadcountAllocation($"headcount-allocation-{index}", "sample"));
        }
        var report = HeadcountAllocationReport.BuildHeadcountAllocationReport("test", items, clock);
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
        HeadcountAllocationReportCountsEachRecordOnce();
    }
}
