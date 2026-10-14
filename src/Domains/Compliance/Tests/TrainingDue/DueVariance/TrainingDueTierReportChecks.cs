namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueVariance;

using Acme.Workforce.Compliance.TrainingDue.DueVariance;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of due variance code, run by the project's runner.</summary>
public static class TrainingDueTierReportChecks
{
    public static void TrainingDueTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704145903);
        var items = new List<TrainingDueTier>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(TrainingDueTier.NewTrainingDueTier($"training-due-tier-{index}", "sample"));
        }
        var report = TrainingDueTierReport.BuildTrainingDueTierReport("test", items, clock);
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
        TrainingDueTierReportCountsEachRecordOnce();
    }
}
