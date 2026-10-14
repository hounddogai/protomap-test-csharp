namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Tier;

using Acme.Workforce.Engagement.PulseSurvey.Tier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class StrictPulseSurveyTierReportChecks
{
    public static void StrictPulseSurveyTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704374853);
        var items = new List<StrictPulseSurveyTier>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(StrictPulseSurveyTier.NewStrictPulseSurveyTier($"strict-pulse-survey-tier-{index}", "sample"));
        }
        var report = StrictPulseSurveyTierReport.BuildStrictPulseSurveyTierReport("test", items, clock);
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
        StrictPulseSurveyTierReportCountsEachRecordOnce();
    }
}
