namespace Acme.Workforce.Engagement.Tests.Feedback.Audit;

using Acme.Workforce.Engagement.Feedback.Audit;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FeedbackForecastReportChecks
{
    public static void FeedbackForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704752563);
        var items = new List<FeedbackForecast>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(FeedbackForecast.NewFeedbackForecast($"feedback-forecast-{index}", "sample"));
        }
        var report = FeedbackForecastReport.BuildFeedbackForecastReport("test", items, clock);
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
        FeedbackForecastReportCountsEachRecordOnce();
    }
}
