namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Quota;

using Acme.Workforce.Engagement.PulseSurvey.Quota;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class ManualPulseSurveyAuditReportChecks
{
    public static void ManualPulseSurveyAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704445973);
        var items = new List<ManualPulseSurveyAudit>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(ManualPulseSurveyAudit.NewManualPulseSurveyAudit($"manual-pulse-survey-audit-{index}", "sample"));
        }
        var report = ManualPulseSurveyAuditReport.BuildManualPulseSurveyAuditReport("test", items, clock);
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
        ManualPulseSurveyAuditReportCountsEachRecordOnce();
    }
}
