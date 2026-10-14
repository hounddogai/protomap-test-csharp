namespace Acme.Workforce.Engagement.Tests.Recognition.OverrideItem;

using Acme.Workforce.Engagement.Recognition.OverrideItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RecognitionOverrideReportChecks
{
    public static void RecognitionOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709558279);
        var items = new List<RecognitionOverride>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(RecognitionOverride.NewRecognitionOverride($"recognition-override-{index}", "sample"));
        }
        var report = RecognitionOverrideReport.BuildRecognitionOverrideReport("test", items, clock);
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
        RecognitionOverrideReportCountsEachRecordOnce();
    }
}
