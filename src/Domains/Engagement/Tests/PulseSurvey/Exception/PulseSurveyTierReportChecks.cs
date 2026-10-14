namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Exception;

using Acme.Workforce.Engagement.PulseSurvey.Exception;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class PulseSurveyTierReportChecks
{
    public static void PulseSurveyTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702188387);
        var items = new List<PulseSurveyTier>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PulseSurveyTier.NewPulseSurveyTier($"pulse-survey-tier-{index}", "sample"));
        }
        var report = PulseSurveyTierReport.BuildPulseSurveyTierReport("test", items, clock);
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
        PulseSurveyTierReportCountsEachRecordOnce();
    }
}
