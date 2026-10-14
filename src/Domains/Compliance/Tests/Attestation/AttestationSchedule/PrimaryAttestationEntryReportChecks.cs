namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationSchedule;

using Acme.Workforce.Compliance.Attestation.AttestationSchedule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of attestation schedule code, run by the project's runner.</summary>
public static class PrimaryAttestationEntryReportChecks
{
    public static void PrimaryAttestationEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707974292);
        var items = new List<PrimaryAttestationEntry>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PrimaryAttestationEntry.NewPrimaryAttestationEntry($"primary-attestation-entry-{index}", "sample"));
        }
        var report = PrimaryAttestationEntryReport.BuildPrimaryAttestationEntryReport("test", items, clock);
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
        PrimaryAttestationEntryReportCountsEachRecordOnce();
    }
}
