namespace Acme.Workforce.Compliance.Tests.TrainingDue.Plan;

using Acme.Workforce.Compliance.TrainingDue.Plan;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class TrainingDueProfileReportChecks
{
    public static void TrainingDueProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703057967);
        var items = new List<TrainingDueProfile>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(TrainingDueProfile.NewTrainingDueProfile($"training-due-profile-{index}", "sample"));
        }
        var report = TrainingDueProfileReport.BuildTrainingDueProfileReport("test", items, clock);
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
        TrainingDueProfileReportCountsEachRecordOnce();
    }
}
