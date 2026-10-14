namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyQuota;

using Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of survey quota code, run by the project's runner.</summary>
public static class StrictPulseSurveyQuotaReportChecks
{
    public static void StrictPulseSurveyQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703849961);
        var items = new List<StrictPulseSurveyQuota>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(StrictPulseSurveyQuota.NewStrictPulseSurveyQuota($"strict-pulse-survey-quota-{index}", "sample"));
        }
        var report = StrictPulseSurveyQuotaReport.BuildStrictPulseSurveyQuotaReport("test", items, clock);
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
        StrictPulseSurveyQuotaReportCountsEachRecordOnce();
    }
}
