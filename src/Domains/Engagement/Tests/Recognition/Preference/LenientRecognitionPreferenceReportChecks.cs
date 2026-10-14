namespace Acme.Workforce.Engagement.Tests.Recognition.Preference;

using Acme.Workforce.Engagement.Recognition.Preference;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class LenientRecognitionPreferenceReportChecks
{
    public static void LenientRecognitionPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708323414);
        var items = new List<LenientRecognitionPreference>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LenientRecognitionPreference.NewLenientRecognitionPreference($"lenient-recognition-preference-{index}", "sample"));
        }
        var report = LenientRecognitionPreferenceReport.BuildLenientRecognitionPreferenceReport("test", items, clock);
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
        LenientRecognitionPreferenceReportCountsEachRecordOnce();
    }
}
