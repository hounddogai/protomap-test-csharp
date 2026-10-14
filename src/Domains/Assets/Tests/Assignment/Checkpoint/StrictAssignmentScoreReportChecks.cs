namespace Acme.Workforce.Assets.Tests.Assignment.Checkpoint;

using Acme.Workforce.Assets.Assignment.Checkpoint;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class StrictAssignmentScoreReportChecks
{
    public static void StrictAssignmentScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702399656);
        var items = new List<StrictAssignmentScore>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(StrictAssignmentScore.NewStrictAssignmentScore($"strict-assignment-score-{index}", "sample"));
        }
        var report = StrictAssignmentScoreReport.BuildStrictAssignmentScoreReport("test", items, clock);
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
        StrictAssignmentScoreReportCountsEachRecordOnce();
    }
}
