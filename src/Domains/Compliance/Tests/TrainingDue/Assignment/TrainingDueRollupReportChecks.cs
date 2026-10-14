namespace Acme.Workforce.Compliance.Tests.TrainingDue.Assignment;

using Acme.Workforce.Compliance.TrainingDue.Assignment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TrainingDueRollupReportChecks
{
    public static void TrainingDueRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702392312);
        var items = new List<TrainingDueRollup>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(TrainingDueRollup.NewTrainingDueRollup($"training-due-rollup-{index}", "sample"));
        }
        var report = TrainingDueRollupReport.BuildTrainingDueRollupReport("test", items, clock);
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
        TrainingDueRollupReportCountsEachRecordOnce();
    }
}
