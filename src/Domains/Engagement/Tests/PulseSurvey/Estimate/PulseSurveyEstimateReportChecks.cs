namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Estimate;

using Acme.Workforce.Engagement.PulseSurvey.Estimate;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PulseSurveyEstimateReportChecks
{
    public static void PulseSurveyEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704389098);
        var items = new List<PulseSurveyEstimate>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PulseSurveyEstimate.NewPulseSurveyEstimate($"pulse-survey-estimate-{index}", "sample"));
        }
        var report = PulseSurveyEstimateReport.BuildPulseSurveyEstimateReport("test", items, clock);
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
        PulseSurveyEstimateReportCountsEachRecordOnce();
    }
}
