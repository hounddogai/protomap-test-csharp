namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyQuota;

using Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of survey quota code, run by the project's runner.</summary>
public static class CappedPulseSurveyForecastReportChecks
{
    public static void CappedPulseSurveyForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702326045);
        var items = new List<CappedPulseSurveyForecast>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(CappedPulseSurveyForecast.NewCappedPulseSurveyForecast($"capped-pulse-survey-forecast-{index}", "sample"));
        }
        var report = CappedPulseSurveyForecastReport.BuildCappedPulseSurveyForecastReport("test", items, clock);
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
        CappedPulseSurveyForecastReportCountsEachRecordOnce();
    }
}
