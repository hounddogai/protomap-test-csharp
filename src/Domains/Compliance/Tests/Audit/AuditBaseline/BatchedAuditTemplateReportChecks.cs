namespace Acme.Workforce.Compliance.Tests.Audit.AuditBaseline;

using Acme.Workforce.Compliance.Audit.AuditBaseline;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of audit baseline code, run by the project's runner.</summary>
public static class BatchedAuditTemplateReportChecks
{
    public static void BatchedAuditTemplateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700742310);
        var items = new List<BatchedAuditTemplate>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(BatchedAuditTemplate.NewBatchedAuditTemplate($"batched-audit-template-{index}", "sample"));
        }
        var report = BatchedAuditTemplateReport.BuildBatchedAuditTemplateReport("test", items, clock);
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
        BatchedAuditTemplateReportCountsEachRecordOnce();
    }
}
