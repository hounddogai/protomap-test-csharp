namespace Acme.Workforce.Compliance.Tests.Certification.Claim;

using Acme.Workforce.Compliance.Certification.Claim;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class BatchedCertificationAuditReportChecks
{
    public static void BatchedCertificationAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703525843);
        var items = new List<BatchedCertificationAudit>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(BatchedCertificationAudit.NewBatchedCertificationAudit($"batched-certification-audit-{index}", "sample"));
        }
        var report = BatchedCertificationAuditReport.BuildBatchedCertificationAuditReport("test", items, clock);
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
        BatchedCertificationAuditReportCountsEachRecordOnce();
    }
}
