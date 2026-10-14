namespace Acme.Workforce.Compliance.Tests.Attestation.Digest;

using Acme.Workforce.Compliance.Attestation.Digest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class AttestationSnapshotReportChecks
{
    public static void AttestationSnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708508054);
        var items = new List<AttestationSnapshot>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AttestationSnapshot.NewAttestationSnapshot($"attestation-snapshot-{index}", "sample"));
        }
        var report = AttestationSnapshotReport.BuildAttestationSnapshotReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        AttestationSnapshotReportCountsEachRecordOnce();
    }
}
