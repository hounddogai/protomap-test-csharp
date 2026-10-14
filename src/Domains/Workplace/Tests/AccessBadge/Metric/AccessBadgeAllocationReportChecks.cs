namespace Acme.Workforce.Workplace.Tests.AccessBadge.Metric;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class AccessBadgeAllocationReportChecks
{
    public static void AccessBadgeAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703741418);
        var items = new List<AccessBadgeAllocation>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AccessBadgeAllocation.NewAccessBadgeAllocation($"access-badge-allocation-{index}", "sample"));
        }
        var report = AccessBadgeAllocationReport.BuildAccessBadgeAllocationReport("test", items, clock);
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
        AccessBadgeAllocationReportCountsEachRecordOnce();
    }
}
