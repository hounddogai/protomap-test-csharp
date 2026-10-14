namespace Acme.Workforce.Engagement.Tests.Recognition.RecognitionVariance;

using Acme.Workforce.Engagement.Recognition.RecognitionVariance;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of recognition variance code, run by the project's runner.</summary>
public static class ManualRecognitionScheduleReportChecks
{
    public static void ManualRecognitionScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702127730);
        var items = new List<ManualRecognitionSchedule>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ManualRecognitionSchedule.NewManualRecognitionSchedule($"manual-recognition-schedule-{index}", "sample"));
        }
        var report = ManualRecognitionScheduleReport.BuildManualRecognitionScheduleReport("test", items, clock);
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
        ManualRecognitionScheduleReportCountsEachRecordOnce();
    }
}
