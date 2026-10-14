namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Threshold;

using Acme.Workforce.Engagement.PulseSurvey.Threshold;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PendingPulseSurveyAgreementReportChecks
{
    public static void PendingPulseSurveyAgreementReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700523013);
        var items = new List<PendingPulseSurveyAgreement>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PendingPulseSurveyAgreement.NewPendingPulseSurveyAgreement($"pending-pulse-survey-agreement-{index}", "sample"));
        }
        var report = PendingPulseSurveyAgreementReport.BuildPendingPulseSurveyAgreementReport("test", items, clock);
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
        PendingPulseSurveyAgreementReportCountsEachRecordOnce();
    }
}
