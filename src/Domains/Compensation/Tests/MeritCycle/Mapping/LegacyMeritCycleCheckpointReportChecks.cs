namespace Acme.Workforce.Compensation.Tests.MeritCycle.Mapping;

using Acme.Workforce.Compensation.MeritCycle.Mapping;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class LegacyMeritCycleCheckpointReportChecks
{
    public static void LegacyMeritCycleCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701636332);
        var items = new List<LegacyMeritCycleCheckpoint>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LegacyMeritCycleCheckpoint.NewLegacyMeritCycleCheckpoint($"legacy-merit-cycle-checkpoint-{index}", "sample"));
        }
        var report = LegacyMeritCycleCheckpointReport.BuildLegacyMeritCycleCheckpointReport("test", items, clock);
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
        LegacyMeritCycleCheckpointReportCountsEachRecordOnce();
    }
}
