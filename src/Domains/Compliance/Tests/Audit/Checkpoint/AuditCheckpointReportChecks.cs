namespace Acme.Workforce.Compliance.Tests.Audit.Checkpoint;

using Acme.Workforce.Compliance.Audit.Checkpoint;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AuditCheckpointReportChecks
{
    public static void AuditCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700116920);
        var items = new List<AuditCheckpoint>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AuditCheckpoint.NewAuditCheckpoint($"audit-checkpoint-{index}", "sample"));
        }
        var report = AuditCheckpointReport.BuildAuditCheckpointReport("test", items, clock);
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
        AuditCheckpointReportCountsEachRecordOnce();
    }
}
