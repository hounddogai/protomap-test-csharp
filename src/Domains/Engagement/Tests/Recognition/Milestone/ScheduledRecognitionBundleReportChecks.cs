namespace Acme.Workforce.Engagement.Tests.Recognition.Milestone;

using Acme.Workforce.Engagement.Recognition.Milestone;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class ScheduledRecognitionBundleReportChecks
{
    public static void ScheduledRecognitionBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707255640);
        var items = new List<ScheduledRecognitionBundle>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ScheduledRecognitionBundle.NewScheduledRecognitionBundle($"scheduled-recognition-bundle-{index}", "sample"));
        }
        var report = ScheduledRecognitionBundleReport.BuildScheduledRecognitionBundleReport("test", items, clock);
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
        ScheduledRecognitionBundleReportCountsEachRecordOnce();
    }
}
