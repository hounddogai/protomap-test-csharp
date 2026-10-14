namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Profile;

using Acme.Workforce.Engagement.PulseSurvey.Profile;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PulseSurveyMilestoneReportChecks
{
    public static void PulseSurveyMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705082207);
        var items = new List<PulseSurveyMilestone>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PulseSurveyMilestone.NewPulseSurveyMilestone($"pulse-survey-milestone-{index}", "sample"));
        }
        var report = PulseSurveyMilestoneReport.BuildPulseSurveyMilestoneReport("test", items, clock);
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
        PulseSurveyMilestoneReportCountsEachRecordOnce();
    }
}
