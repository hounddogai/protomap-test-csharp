namespace Acme.Workforce.Engagement.Tests.Recognition.Schedule;

using Acme.Workforce.Engagement.Recognition.Schedule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class RollingRecognitionSlotReportChecks
{
    public static void RollingRecognitionSlotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702077809);
        var items = new List<RollingRecognitionSlot>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RollingRecognitionSlot.NewRollingRecognitionSlot($"rolling-recognition-slot-{index}", "sample"));
        }
        var report = RollingRecognitionSlotReport.BuildRollingRecognitionSlotReport("test", items, clock);
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
        RollingRecognitionSlotReportCountsEachRecordOnce();
    }
}
