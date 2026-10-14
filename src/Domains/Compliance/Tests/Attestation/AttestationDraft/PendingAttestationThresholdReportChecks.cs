namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationDraft;

using Acme.Workforce.Compliance.Attestation.AttestationDraft;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of attestation draft code, run by the project's runner.</summary>
public static class PendingAttestationThresholdReportChecks
{
    public static void PendingAttestationThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702450212);
        var items = new List<PendingAttestationThreshold>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PendingAttestationThreshold.NewPendingAttestationThreshold($"pending-attestation-threshold-{index}", "sample"));
        }
        var report = PendingAttestationThresholdReport.BuildPendingAttestationThresholdReport("test", items, clock);
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
        PendingAttestationThresholdReportCountsEachRecordOnce();
    }
}
