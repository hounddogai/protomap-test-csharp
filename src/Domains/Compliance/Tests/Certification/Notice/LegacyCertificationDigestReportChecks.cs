namespace Acme.Workforce.Compliance.Tests.Certification.Notice;

using Acme.Workforce.Compliance.Certification.Notice;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class LegacyCertificationDigestReportChecks
{
    public static void LegacyCertificationDigestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705272002);
        var items = new List<LegacyCertificationDigest>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(LegacyCertificationDigest.NewLegacyCertificationDigest($"legacy-certification-digest-{index}", "sample"));
        }
        var report = LegacyCertificationDigestReport.BuildLegacyCertificationDigestReport("test", items, clock);
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
        LegacyCertificationDigestReportCountsEachRecordOnce();
    }
}
