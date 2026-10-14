namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Template;

using Acme.Workforce.Engagement.PulseSurvey.Template;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class PulseSurveyTemplateReportChecks
{
    public static void PulseSurveyTemplateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707819133);
        var items = new List<PulseSurveyTemplate>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PulseSurveyTemplate.NewPulseSurveyTemplate($"pulse-survey-template-{index}", "sample"));
        }
        var report = PulseSurveyTemplateReport.BuildPulseSurveyTemplateReport("test", items, clock);
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
        PulseSurveyTemplateReportCountsEachRecordOnce();
    }
}
