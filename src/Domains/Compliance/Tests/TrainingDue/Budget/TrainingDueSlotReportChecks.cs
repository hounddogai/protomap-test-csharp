namespace Acme.Workforce.Compliance.Tests.TrainingDue.Budget;

using Acme.Workforce.Compliance.TrainingDue.Budget;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class TrainingDueSlotReportChecks
{
    public static void TrainingDueSlotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706734388);
        var items = new List<TrainingDueSlot>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(TrainingDueSlot.NewTrainingDueSlot($"training-due-slot-{index}", "sample"));
        }
        var report = TrainingDueSlotReport.BuildTrainingDueSlotReport("test", items, clock);
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
        TrainingDueSlotReportCountsEachRecordOnce();
    }
}
