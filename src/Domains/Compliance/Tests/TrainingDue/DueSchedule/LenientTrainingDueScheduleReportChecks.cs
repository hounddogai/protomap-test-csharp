namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueSchedule;

using Acme.Workforce.Compliance.TrainingDue.DueSchedule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of due schedule code, run by the project's runner.</summary>
public static class LenientTrainingDueScheduleReportChecks
{
    public static void LenientTrainingDueScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700423179);
        var items = new List<LenientTrainingDueSchedule>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LenientTrainingDueSchedule.NewLenientTrainingDueSchedule($"lenient-training-due-schedule-{index}", "sample"));
        }
        var report = LenientTrainingDueScheduleReport.BuildLenientTrainingDueScheduleReport("test", items, clock);
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
        LenientTrainingDueScheduleReportCountsEachRecordOnce();
    }
}
