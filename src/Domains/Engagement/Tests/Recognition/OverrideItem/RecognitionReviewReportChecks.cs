namespace Acme.Workforce.Engagement.Tests.Recognition.OverrideItem;

using Acme.Workforce.Engagement.Recognition.OverrideItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RecognitionReviewReportChecks
{
    public static void RecognitionReviewReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708114467);
        var items = new List<RecognitionReview>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(RecognitionReview.NewRecognitionReview($"recognition-review-{index}", "sample"));
        }
        var report = RecognitionReviewReport.BuildRecognitionReviewReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        RecognitionReviewReportCountsEachRecordOnce();
    }
}
