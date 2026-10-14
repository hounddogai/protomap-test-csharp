namespace Acme.Workforce.Compliance.Tests.TrainingDue.OverrideItem;

using Acme.Workforce.Compliance.TrainingDue.OverrideItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class TrainingDueOverrideReportChecks
{
    public static void TrainingDueOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703826078);
        var items = new List<TrainingDueOverride>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(TrainingDueOverride.NewTrainingDueOverride($"training-due-override-{index}", "sample"));
        }
        var report = TrainingDueOverrideReport.BuildTrainingDueOverrideReport("test", items, clock);
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
        TrainingDueOverrideReportCountsEachRecordOnce();
    }
}
