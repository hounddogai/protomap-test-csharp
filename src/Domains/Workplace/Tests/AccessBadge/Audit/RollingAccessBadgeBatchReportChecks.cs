namespace Acme.Workforce.Workplace.Tests.AccessBadge.Audit;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class RollingAccessBadgeBatchReportChecks
{
    public static void RollingAccessBadgeBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709454558);
        var items = new List<RollingAccessBadgeBatch>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(RollingAccessBadgeBatch.NewRollingAccessBadgeBatch($"rolling-access-badge-batch-{index}", "sample"));
        }
        var report = RollingAccessBadgeBatchReport.BuildRollingAccessBadgeBatchReport("test", items, clock);
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
        RollingAccessBadgeBatchReportCountsEachRecordOnce();
    }
}
