namespace Acme.Workforce.Compliance.Tests.Certification.Assignment;

using Acme.Workforce.Compliance.Certification.Assignment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class CertificationCheckpointReportChecks
{
    public static void CertificationCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704487546);
        var items = new List<CertificationCheckpoint>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CertificationCheckpoint.NewCertificationCheckpoint($"certification-checkpoint-{index}", "sample"));
        }
        var report = CertificationCheckpointReport.BuildCertificationCheckpointReport("test", items, clock);
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
        CertificationCheckpointReportCountsEachRecordOnce();
    }
}
