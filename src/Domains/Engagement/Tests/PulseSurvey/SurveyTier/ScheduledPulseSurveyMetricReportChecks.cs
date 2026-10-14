namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyTier;

using Acme.Workforce.Engagement.PulseSurvey.SurveyTier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of survey tier code, run by the project's runner.</summary>
public static class ScheduledPulseSurveyMetricReportChecks
{
    public static void ScheduledPulseSurveyMetricReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706358962);
        var items = new List<ScheduledPulseSurveyMetric>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ScheduledPulseSurveyMetric.NewScheduledPulseSurveyMetric($"scheduled-pulse-survey-metric-{index}", "sample"));
        }
        var report = ScheduledPulseSurveyMetricReport.BuildScheduledPulseSurveyMetricReport("test", items, clock);
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
        ScheduledPulseSurveyMetricReportCountsEachRecordOnce();
    }
}
