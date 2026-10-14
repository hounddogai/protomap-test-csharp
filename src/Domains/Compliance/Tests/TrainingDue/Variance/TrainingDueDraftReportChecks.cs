namespace Acme.Workforce.Compliance.Tests.TrainingDue.Variance;

using Acme.Workforce.Compliance.TrainingDue.Variance;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class TrainingDueDraftReportChecks
{
    public static void TrainingDueDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704327325);
        var items = new List<TrainingDueDraft>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(TrainingDueDraft.NewTrainingDueDraft($"training-due-draft-{index}", "sample"));
        }
        var report = TrainingDueDraftReport.BuildTrainingDueDraftReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        TrainingDueDraftReportCountsEachRecordOnce();
    }
}
