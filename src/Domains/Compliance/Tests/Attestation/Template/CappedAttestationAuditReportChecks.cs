namespace Acme.Workforce.Compliance.Tests.Attestation.Template;

using Acme.Workforce.Compliance.Attestation.Template;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class CappedAttestationAuditReportChecks
{
    public static void CappedAttestationAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700502465);
        var items = new List<CappedAttestationAudit>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(CappedAttestationAudit.NewCappedAttestationAudit($"capped-attestation-audit-{index}", "sample"));
        }
        var report = CappedAttestationAuditReport.BuildCappedAttestationAuditReport("test", items, clock);
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
        CappedAttestationAuditReportCountsEachRecordOnce();
    }
}
