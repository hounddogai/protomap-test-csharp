namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Template;

using Acme.Workforce.Engagement.PulseSurvey.Template;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class PulseSurveyNoticeReportChecks
{
    public static void PulseSurveyNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701884298);
        var items = new List<PulseSurveyNotice>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PulseSurveyNotice.NewPulseSurveyNotice($"pulse-survey-notice-{index}", "sample"));
        }
        var report = PulseSurveyNoticeReport.BuildPulseSurveyNoticeReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        PulseSurveyNoticeReportCountsEachRecordOnce();
    }
}
