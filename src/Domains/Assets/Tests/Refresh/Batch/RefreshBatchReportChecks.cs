namespace Acme.Workforce.Assets.Tests.Refresh.Batch;

using Acme.Workforce.Assets.Refresh.Batch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class RefreshBatchReportChecks
{
    public static void RefreshBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707165417);
        var items = new List<RefreshBatch>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RefreshBatch.NewRefreshBatch($"refresh-batch-{index}", "sample"));
        }
        var report = RefreshBatchReport.BuildRefreshBatchReport("test", items, clock);
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
        RefreshBatchReportCountsEachRecordOnce();
    }
}
