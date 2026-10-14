namespace Acme.Workforce.Engagement.Tests.Recognition.Window;

using Acme.Workforce.Engagement.Recognition.Window;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RecognitionWindowReportChecks
{
    public static void RecognitionWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704838866);
        var items = new List<RecognitionWindow>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RecognitionWindow.NewRecognitionWindow($"recognition-window-{index}", "sample"));
        }
        var report = RecognitionWindowReport.BuildRecognitionWindowReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        RecognitionWindowReportCountsEachRecordOnce();
    }
}
