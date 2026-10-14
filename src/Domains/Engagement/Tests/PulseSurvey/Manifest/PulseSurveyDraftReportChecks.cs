namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Manifest;

using Acme.Workforce.Engagement.PulseSurvey.Manifest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class PulseSurveyDraftReportChecks
{
    public static void PulseSurveyDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706217779);
        var items = new List<PulseSurveyDraft>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PulseSurveyDraft.NewPulseSurveyDraft($"pulse-survey-draft-{index}", "sample"));
        }
        var report = PulseSurveyDraftReport.BuildPulseSurveyDraftReport("test", items, clock);
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
        PulseSurveyDraftReportCountsEachRecordOnce();
    }
}
