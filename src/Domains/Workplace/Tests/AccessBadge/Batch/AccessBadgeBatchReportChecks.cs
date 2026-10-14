namespace Acme.Workforce.Workplace.Tests.AccessBadge.Batch;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class AccessBadgeBatchReportChecks
{
    public static void AccessBadgeBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705119152);
        var items = new List<AccessBadgeBatch>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(AccessBadgeBatch.NewAccessBadgeBatch($"access-badge-batch-{index}", "sample"));
        }
        var report = AccessBadgeBatchReport.BuildAccessBadgeBatchReport("test", items, clock);
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
        AccessBadgeBatchReportCountsEachRecordOnce();
    }
}
