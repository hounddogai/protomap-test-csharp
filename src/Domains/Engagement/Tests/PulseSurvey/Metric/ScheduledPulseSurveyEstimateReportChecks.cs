namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Metric;

using Acme.Workforce.Engagement.PulseSurvey.Metric;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ScheduledPulseSurveyEstimateReportChecks
{
    public static void ScheduledPulseSurveyEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700106071);
        var items = new List<ScheduledPulseSurveyEstimate>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ScheduledPulseSurveyEstimate.NewScheduledPulseSurveyEstimate($"scheduled-pulse-survey-estimate-{index}", "sample"));
        }
        var report = ScheduledPulseSurveyEstimateReport.BuildScheduledPulseSurveyEstimateReport("test", items, clock);
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
        ScheduledPulseSurveyEstimateReportCountsEachRecordOnce();
    }
}
