namespace Acme.Workforce.Compliance.Tests.Audit.Batch;

using Acme.Workforce.Compliance.Audit.Batch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class AuditBatchReportChecks
{
    public static void AuditBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703826392);
        var items = new List<AuditBatch>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AuditBatch.NewAuditBatch($"audit-batch-{index}", "sample"));
        }
        var report = AuditBatchReport.BuildAuditBatchReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        AuditBatchReportCountsEachRecordOnce();
    }
}
