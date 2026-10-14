namespace Acme.Workforce.Compliance.Tests.Certification.Request;

using Acme.Workforce.Compliance.Certification.Request;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class CertificationRequestReportChecks
{
    public static void CertificationRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701604131);
        var items = new List<CertificationRequest>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(CertificationRequest.NewCertificationRequest($"certification-request-{index}", "sample"));
        }
        var report = CertificationRequestReport.BuildCertificationRequestReport("test", items, clock);
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
        CertificationRequestReportCountsEachRecordOnce();
    }
}
