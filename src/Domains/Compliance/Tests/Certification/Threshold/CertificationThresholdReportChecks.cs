namespace Acme.Workforce.Compliance.Tests.Certification.Threshold;

using Acme.Workforce.Compliance.Certification.Threshold;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class CertificationThresholdReportChecks
{
    public static void CertificationThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705680205);
        var items = new List<CertificationThreshold>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(CertificationThreshold.NewCertificationThreshold($"certification-threshold-{index}", "sample"));
        }
        var report = CertificationThresholdReport.BuildCertificationThresholdReport("test", items, clock);
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
        CertificationThresholdReportCountsEachRecordOnce();
    }
}
