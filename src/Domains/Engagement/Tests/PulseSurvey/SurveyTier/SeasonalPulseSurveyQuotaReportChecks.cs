namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyTier;

using Acme.Workforce.Engagement.PulseSurvey.SurveyTier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of survey tier code, run by the project's runner.</summary>
public static class SeasonalPulseSurveyQuotaReportChecks
{
    public static void SeasonalPulseSurveyQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709193832);
        var items = new List<SeasonalPulseSurveyQuota>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(SeasonalPulseSurveyQuota.NewSeasonalPulseSurveyQuota($"seasonal-pulse-survey-quota-{index}", "sample"));
        }
        var report = SeasonalPulseSurveyQuotaReport.BuildSeasonalPulseSurveyQuotaReport("test", items, clock);
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
        SeasonalPulseSurveyQuotaReportCountsEachRecordOnce();
    }
}
