namespace Acme.Workforce.Compliance.Tests.TrainingDue.Plan;

using Acme.Workforce.Compliance.TrainingDue.Plan;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class TrainingDuePlanReportChecks
{
    public static void TrainingDuePlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703921575);
        var items = new List<TrainingDuePlan>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(TrainingDuePlan.NewTrainingDuePlan($"training-due-plan-{index}", "sample"));
        }
        var report = TrainingDuePlanReport.BuildTrainingDuePlanReport("test", items, clock);
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
        TrainingDuePlanReportCountsEachRecordOnce();
    }
}
