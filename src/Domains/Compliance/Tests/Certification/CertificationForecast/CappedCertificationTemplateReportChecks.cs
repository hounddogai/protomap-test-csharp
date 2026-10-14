namespace Acme.Workforce.Compliance.Tests.Certification.CertificationForecast;

using Acme.Workforce.Compliance.Certification.CertificationForecast;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of certification forecast code, run by the project's runner.</summary>
public static class CappedCertificationTemplateReportChecks
{
    public static void CappedCertificationTemplateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707795106);
        var items = new List<CappedCertificationTemplate>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(CappedCertificationTemplate.NewCappedCertificationTemplate($"capped-certification-template-{index}", "sample"));
        }
        var report = CappedCertificationTemplateReport.BuildCappedCertificationTemplateReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        CappedCertificationTemplateReportCountsEachRecordOnce();
    }
}
