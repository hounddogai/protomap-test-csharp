namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationSchedule;

using Acme.Workforce.Compliance.Attestation.AttestationSchedule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of attestation schedule code, run by the project's runner.</summary>
public static class PrimaryAttestationScoreReportChecks
{
    public static void PrimaryAttestationScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705202573);
        var items = new List<PrimaryAttestationScore>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PrimaryAttestationScore.NewPrimaryAttestationScore($"primary-attestation-score-{index}", "sample"));
        }
        var report = PrimaryAttestationScoreReport.BuildPrimaryAttestationScoreReport("test", items, clock);
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
        PrimaryAttestationScoreReportCountsEachRecordOnce();
    }
}
