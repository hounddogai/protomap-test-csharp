namespace Acme.Workforce.Compliance.Tests.Certification.Checkpoint;

using Acme.Workforce.Compliance.Certification.Checkpoint;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class FallbackCertificationCheckpointReportChecks
{
    public static void FallbackCertificationCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701191260);
        var items = new List<FallbackCertificationCheckpoint>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(FallbackCertificationCheckpoint.NewFallbackCertificationCheckpoint($"fallback-certification-checkpoint-{index}", "sample"));
        }
        var report = FallbackCertificationCheckpointReport.BuildFallbackCertificationCheckpointReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        FallbackCertificationCheckpointReportCountsEachRecordOnce();
    }
}
