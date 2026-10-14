namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Threshold;

using Acme.Workforce.Engagement.PulseSurvey.Threshold;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PulseSurveyThresholdReportChecks
{
    public static void PulseSurveyThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703921465);
        var items = new List<PulseSurveyThreshold>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PulseSurveyThreshold.NewPulseSurveyThreshold($"pulse-survey-threshold-{index}", "sample"));
        }
        var report = PulseSurveyThresholdReport.BuildPulseSurveyThresholdReport("test", items, clock);
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
        PulseSurveyThresholdReportCountsEachRecordOnce();
    }
}
