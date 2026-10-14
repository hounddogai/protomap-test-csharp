namespace Acme.Workforce.Compliance.Tests.Certification.Checkpoint;

using Acme.Workforce.Compliance.Certification.Checkpoint;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LenientCertificationTierReportChecks
{
    public static void LenientCertificationTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704132332);
        var items = new List<LenientCertificationTier>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LenientCertificationTier.NewLenientCertificationTier($"lenient-certification-tier-{index}", "sample"));
        }
        var report = LenientCertificationTierReport.BuildLenientCertificationTierReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        LenientCertificationTierReportCountsEachRecordOnce();
    }
}
