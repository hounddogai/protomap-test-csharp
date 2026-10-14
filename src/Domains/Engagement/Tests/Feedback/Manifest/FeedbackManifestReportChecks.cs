namespace Acme.Workforce.Engagement.Tests.Feedback.Manifest;

using Acme.Workforce.Engagement.Feedback.Manifest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class FeedbackManifestReportChecks
{
    public static void FeedbackManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706319077);
        var items = new List<FeedbackManifest>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(FeedbackManifest.NewFeedbackManifest($"feedback-manifest-{index}", "sample"));
        }
        var report = FeedbackManifestReport.BuildFeedbackManifestReport("test", items, clock);
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
        FeedbackManifestReportCountsEachRecordOnce();
    }
}
