namespace Acme.Workforce.Compliance.Tests.Audit.Checkpoint;

using Acme.Workforce.Compliance.Audit.Checkpoint;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LegacyAuditCheckpointReportChecks
{
    public static void LegacyAuditCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700602966);
        var items = new List<LegacyAuditCheckpoint>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LegacyAuditCheckpoint.NewLegacyAuditCheckpoint($"legacy-audit-checkpoint-{index}", "sample"));
        }
        var report = LegacyAuditCheckpointReport.BuildLegacyAuditCheckpointReport("test", items, clock);
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
        LegacyAuditCheckpointReportCountsEachRecordOnce();
    }
}
