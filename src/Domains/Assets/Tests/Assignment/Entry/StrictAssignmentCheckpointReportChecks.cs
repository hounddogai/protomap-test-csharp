namespace Acme.Workforce.Assets.Tests.Assignment.Entry;

using Acme.Workforce.Assets.Assignment.Entry;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class StrictAssignmentCheckpointReportChecks
{
    public static void StrictAssignmentCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706033430);
        var items = new List<StrictAssignmentCheckpoint>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(StrictAssignmentCheckpoint.NewStrictAssignmentCheckpoint($"strict-assignment-checkpoint-{index}", "sample"));
        }
        var report = StrictAssignmentCheckpointReport.BuildStrictAssignmentCheckpointReport("test", items, clock);
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
        StrictAssignmentCheckpointReportCountsEachRecordOnce();
    }
}
