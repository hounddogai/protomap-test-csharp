namespace Acme.Workforce.Workplace.Tests.AccessBadge.Policy;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class CappedAccessBadgeThresholdReportChecks
{
    public static void CappedAccessBadgeThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708945931);
        var items = new List<CappedAccessBadgeThreshold>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CappedAccessBadgeThreshold.NewCappedAccessBadgeThreshold($"capped-access-badge-threshold-{index}", "sample"));
        }
        var report = CappedAccessBadgeThresholdReport.BuildCappedAccessBadgeThresholdReport("test", items, clock);
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
        CappedAccessBadgeThresholdReportCountsEachRecordOnce();
    }
}
