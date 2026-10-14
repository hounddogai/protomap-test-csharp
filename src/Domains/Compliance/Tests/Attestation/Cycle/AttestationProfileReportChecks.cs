namespace Acme.Workforce.Compliance.Tests.Attestation.Cycle;

using Acme.Workforce.Compliance.Attestation.Cycle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AttestationProfileReportChecks
{
    public static void AttestationProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707106025);
        var items = new List<AttestationProfile>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AttestationProfile.NewAttestationProfile($"attestation-profile-{index}", "sample"));
        }
        var report = AttestationProfileReport.BuildAttestationProfileReport("test", items, clock);
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
        AttestationProfileReportCountsEachRecordOnce();
    }
}
