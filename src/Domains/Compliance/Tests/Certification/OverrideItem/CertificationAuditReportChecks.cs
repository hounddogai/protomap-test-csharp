namespace Acme.Workforce.Compliance.Tests.Certification.OverrideItem;

using Acme.Workforce.Compliance.Certification.OverrideItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class CertificationAuditReportChecks
{
    public static void CertificationAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704858026);
        var items = new List<CertificationAudit>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(CertificationAudit.NewCertificationAudit($"certification-audit-{index}", "sample"));
        }
        var report = CertificationAuditReport.BuildCertificationAuditReport("test", items, clock);
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
        CertificationAuditReportCountsEachRecordOnce();
    }
}
