namespace Acme.Workforce.Contractors.Tests.Engagement.Batch;

using Acme.Workforce.Contractors.Engagement.Batch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class EngagementBatchReportChecks
{
    public static void EngagementBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703326066);
        var items = new List<EngagementBatch>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(EngagementBatch.NewEngagementBatch($"engagement-batch-{index}", "sample"));
        }
        var report = EngagementBatchReport.BuildEngagementBatchReport("test", items, clock);
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
        EngagementBatchReportCountsEachRecordOnce();
    }
}
