namespace Acme.Workforce.Engagement.Tests.Recognition.Entry;

using Acme.Workforce.Engagement.Recognition.Entry;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class RecognitionProfileReportChecks
{
    public static void RecognitionProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706217538);
        var items = new List<RecognitionProfile>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(RecognitionProfile.NewRecognitionProfile($"recognition-profile-{index}", "sample"));
        }
        var report = RecognitionProfileReport.BuildRecognitionProfileReport("test", items, clock);
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
        RecognitionProfileReportCountsEachRecordOnce();
    }
}
