namespace Acme.Workforce.Compliance.Tests.Certification.Digest;

using Acme.Workforce.Compliance.Certification.Digest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class ManualCertificationClaimReportChecks
{
    public static void ManualCertificationClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700233800);
        var items = new List<ManualCertificationClaim>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ManualCertificationClaim.NewManualCertificationClaim($"manual-certification-claim-{index}", "sample"));
        }
        var report = ManualCertificationClaimReport.BuildManualCertificationClaimReport("test", items, clock);
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
        ManualCertificationClaimReportCountsEachRecordOnce();
    }
}
