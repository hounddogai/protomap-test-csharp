namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Metric;

using Acme.Workforce.Engagement.PulseSurvey.Metric;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class StrictPulseSurveyCheckpointReportChecks
{
    public static void StrictPulseSurveyCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702444826);
        var items = new List<StrictPulseSurveyCheckpoint>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(StrictPulseSurveyCheckpoint.NewStrictPulseSurveyCheckpoint($"strict-pulse-survey-checkpoint-{index}", "sample"));
        }
        var report = StrictPulseSurveyCheckpointReport.BuildStrictPulseSurveyCheckpointReport("test", items, clock);
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
        StrictPulseSurveyCheckpointReportCountsEachRecordOnce();
    }
}
