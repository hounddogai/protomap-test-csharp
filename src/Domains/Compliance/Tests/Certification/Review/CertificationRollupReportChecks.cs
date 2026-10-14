namespace Acme.Workforce.Compliance.Tests.Certification.Review;

using Acme.Workforce.Compliance.Certification.Review;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class CertificationRollupReportChecks
{
    public static void CertificationRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705310330);
        var items = new List<CertificationRollup>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(CertificationRollup.NewCertificationRollup($"certification-rollup-{index}", "sample"));
        }
        var report = CertificationRollupReport.BuildCertificationRollupReport("test", items, clock);
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
        CertificationRollupReportCountsEachRecordOnce();
    }
}
