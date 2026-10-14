namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Estimate;

using Acme.Workforce.Engagement.PulseSurvey.Estimate;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PulseSurveyForecastReportChecks
{
    public static void PulseSurveyForecastReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705433174);
        var items = new List<PulseSurveyForecast>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PulseSurveyForecast.NewPulseSurveyForecast($"pulse-survey-forecast-{index}", "sample"));
        }
        var report = PulseSurveyForecastReport.BuildPulseSurveyForecastReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        PulseSurveyForecastReportCountsEachRecordOnce();
    }
}
