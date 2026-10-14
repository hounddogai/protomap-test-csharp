namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Quota;

using Acme.Workforce.Engagement.PulseSurvey.Quota;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class ManualPulseSurveyQuotaReportChecks
{
    public static void ManualPulseSurveyQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701598286);
        var items = new List<ManualPulseSurveyQuota>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ManualPulseSurveyQuota.NewManualPulseSurveyQuota($"manual-pulse-survey-quota-{index}", "sample"));
        }
        var report = ManualPulseSurveyQuotaReport.BuildManualPulseSurveyQuotaReport("test", items, clock);
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
        ManualPulseSurveyQuotaReportCountsEachRecordOnce();
    }
}
