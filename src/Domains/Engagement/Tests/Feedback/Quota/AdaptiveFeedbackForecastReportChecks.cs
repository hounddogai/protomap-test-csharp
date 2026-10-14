namespace Acme.Workforce.Engagement.Tests.Feedback.Quota;

using Acme.Workforce.Engagement.Feedback.Quota;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AdaptiveFeedbackForecastReportChecks
{
    public static void AdaptiveFeedbackForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706848715);
        var items = new List<AdaptiveFeedbackForecast>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AdaptiveFeedbackForecast.NewAdaptiveFeedbackForecast($"adaptive-feedback-forecast-{index}", "sample"));
        }
        var report = AdaptiveFeedbackForecastReport.BuildAdaptiveFeedbackForecastReport("test", items, clock);
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
        AdaptiveFeedbackForecastReportCountsEachRecordOnce();
    }
}
