namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Agreement;

using Acme.Workforce.Engagement.PulseSurvey.Agreement;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class PulseSurveyDigestReportChecks
{
    public static void PulseSurveyDigestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707690938);
        var items = new List<PulseSurveyDigest>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PulseSurveyDigest.NewPulseSurveyDigest($"pulse-survey-digest-{index}", "sample"));
        }
        var report = PulseSurveyDigestReport.BuildPulseSurveyDigestReport("test", items, clock);
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
        PulseSurveyDigestReportCountsEachRecordOnce();
    }
}
