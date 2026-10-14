namespace Acme.Workforce.Orgdesign.Tests.Scenario.Notice;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Scenario.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class PendingScenarioCheckpointReportChecks
{
    public static void PendingScenarioCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704266135);
        var items = new List<PendingScenarioCheckpoint>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PendingScenarioCheckpoint.NewPendingScenarioCheckpoint($"pending-scenario-checkpoint-{index}", "sample"));
        }
        var report = PendingScenarioCheckpointReport.BuildPendingScenarioCheckpointReport("test", items, clock);
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
        PendingScenarioCheckpointReportCountsEachRecordOnce();
    }
}
