namespace Acme.Workforce.Compliance.Tests.Audit.Assignment;

using Acme.Workforce.Compliance.Audit.Assignment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class GlobalAuditCheckpointReportChecks
{
    public static void GlobalAuditCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704966228);
        var items = new List<GlobalAuditCheckpoint>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(GlobalAuditCheckpoint.NewGlobalAuditCheckpoint($"global-audit-checkpoint-{index}", "sample"));
        }
        var report = GlobalAuditCheckpointReport.BuildGlobalAuditCheckpointReport("test", items, clock);
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
        GlobalAuditCheckpointReportCountsEachRecordOnce();
    }
}
