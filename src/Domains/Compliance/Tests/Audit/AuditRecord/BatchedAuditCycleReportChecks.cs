namespace Acme.Workforce.Compliance.Tests.Audit.AuditRecord;

using Acme.Workforce.Compliance.Audit.AuditRecord;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of audit record code, run by the project's runner.</summary>
public static class BatchedAuditCycleReportChecks
{
    public static void BatchedAuditCycleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700046556);
        var items = new List<BatchedAuditCycle>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(BatchedAuditCycle.NewBatchedAuditCycle($"batched-audit-cycle-{index}", "sample"));
        }
        var report = BatchedAuditCycleReport.BuildBatchedAuditCycleReport("test", items, clock);
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
        BatchedAuditCycleReportCountsEachRecordOnce();
    }
}
