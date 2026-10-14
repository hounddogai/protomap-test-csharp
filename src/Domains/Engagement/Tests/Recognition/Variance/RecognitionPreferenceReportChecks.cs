namespace Acme.Workforce.Engagement.Tests.Recognition.Variance;

using Acme.Workforce.Engagement.Recognition.Variance;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class RecognitionPreferenceReportChecks
{
    public static void RecognitionPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708480032);
        var items = new List<RecognitionPreference>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RecognitionPreference.NewRecognitionPreference($"recognition-preference-{index}", "sample"));
        }
        var report = RecognitionPreferenceReport.BuildRecognitionPreferenceReport("test", items, clock);
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
        RecognitionPreferenceReportCountsEachRecordOnce();
    }
}
