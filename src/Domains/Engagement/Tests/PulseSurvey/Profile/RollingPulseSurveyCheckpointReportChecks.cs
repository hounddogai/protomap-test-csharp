namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Profile;

using Acme.Workforce.Engagement.PulseSurvey.Profile;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class RollingPulseSurveyCheckpointReportChecks
{
    public static void RollingPulseSurveyCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709967348);
        var items = new List<RollingPulseSurveyCheckpoint>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RollingPulseSurveyCheckpoint.NewRollingPulseSurveyCheckpoint($"rolling-pulse-survey-checkpoint-{index}", "sample"));
        }
        var report = RollingPulseSurveyCheckpointReport.BuildRollingPulseSurveyCheckpointReport("test", items, clock);
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
        RollingPulseSurveyCheckpointReportCountsEachRecordOnce();
    }
}
