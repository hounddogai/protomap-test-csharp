namespace Acme.Workforce.Talent.Tests.Nomination.Checkpoint;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class NominationCheckpointReportChecks
{
    public static void NominationCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700504379);
        var items = new List<NominationCheckpoint>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(NominationCheckpoint.NewNominationCheckpoint($"nomination-checkpoint-{index}", "sample"));
        }
        var report = NominationCheckpointReport.BuildNominationCheckpointReport("test", items, clock);
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
        NominationCheckpointReportCountsEachRecordOnce();
    }
}
