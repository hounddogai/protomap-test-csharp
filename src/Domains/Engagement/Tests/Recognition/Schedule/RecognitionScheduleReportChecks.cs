namespace Acme.Workforce.Engagement.Tests.Recognition.Schedule;

using Acme.Workforce.Engagement.Recognition.Schedule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class RecognitionScheduleReportChecks
{
    public static void RecognitionScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701213788);
        var items = new List<RecognitionSchedule>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(RecognitionSchedule.NewRecognitionSchedule($"recognition-schedule-{index}", "sample"));
        }
        var report = RecognitionScheduleReport.BuildRecognitionScheduleReport("test", items, clock);
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
        RecognitionScheduleReportCountsEachRecordOnce();
    }
}
