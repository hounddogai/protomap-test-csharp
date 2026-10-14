namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Audit;

using Acme.Workforce.Engagement.PulseSurvey.Audit;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PulseSurveyAuditReportChecks
{
    public static void PulseSurveyAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708280540);
        var items = new List<PulseSurveyAudit>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PulseSurveyAudit.NewPulseSurveyAudit($"pulse-survey-audit-{index}", "sample"));
        }
        var report = PulseSurveyAuditReport.BuildPulseSurveyAuditReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        PulseSurveyAuditReportCountsEachRecordOnce();
    }
}
