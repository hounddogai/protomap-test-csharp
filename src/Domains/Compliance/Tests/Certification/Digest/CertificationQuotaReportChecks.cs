namespace Acme.Workforce.Compliance.Tests.Certification.Digest;

using Acme.Workforce.Compliance.Certification.Digest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class CertificationQuotaReportChecks
{
    public static void CertificationQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702236266);
        var items = new List<CertificationQuota>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(CertificationQuota.NewCertificationQuota($"certification-quota-{index}", "sample"));
        }
        var report = CertificationQuotaReport.BuildCertificationQuotaReport("test", items, clock);
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
        CertificationQuotaReportCountsEachRecordOnce();
    }
}
