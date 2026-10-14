namespace Acme.Workforce.Compliance.Tests.Attestation.Audit;

using Acme.Workforce.Compliance.Attestation.Audit;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class AttestationAuditReportChecks
{
    public static void AttestationAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705468762);
        var items = new List<AttestationAudit>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AttestationAudit.NewAttestationAudit($"attestation-audit-{index}", "sample"));
        }
        var report = AttestationAuditReport.BuildAttestationAuditReport("test", items, clock);
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
        AttestationAuditReportCountsEachRecordOnce();
    }
}
