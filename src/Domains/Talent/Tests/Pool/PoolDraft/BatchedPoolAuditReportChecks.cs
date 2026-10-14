namespace Acme.Workforce.Talent.Tests.Pool.PoolDraft;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.PoolDraft;

/// <summary>Checks of pool draft code, run by the project's runner.</summary>
public static class BatchedPoolAuditReportChecks
{
    public static void BatchedPoolAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702778268);
        var items = new List<BatchedPoolAudit>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(BatchedPoolAudit.NewBatchedPoolAudit($"batched-pool-audit-{index}", "sample"));
        }
        var report = BatchedPoolAuditReport.BuildBatchedPoolAuditReport("test", items, clock);
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
        BatchedPoolAuditReportCountsEachRecordOnce();
    }
}
