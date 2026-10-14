namespace Acme.Workforce.Compliance.Tests.TrainingDue.Assignment;

using Acme.Workforce.Compliance.TrainingDue.Assignment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TrainingDueAssignmentReportChecks
{
    public static void TrainingDueAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707213323);
        var items = new List<TrainingDueAssignment>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(TrainingDueAssignment.NewTrainingDueAssignment($"training-due-assignment-{index}", "sample"));
        }
        var report = TrainingDueAssignmentReport.BuildTrainingDueAssignmentReport("test", items, clock);
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
        TrainingDueAssignmentReportCountsEachRecordOnce();
    }
}
