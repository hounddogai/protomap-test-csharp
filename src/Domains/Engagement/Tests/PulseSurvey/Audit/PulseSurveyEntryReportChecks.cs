namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Audit;

using Acme.Workforce.Engagement.PulseSurvey.Audit;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PulseSurveyEntryReportChecks
{
    public static void PulseSurveyEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709997237);
        var items = new List<PulseSurveyEntry>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PulseSurveyEntry.NewPulseSurveyEntry($"pulse-survey-entry-{index}", "sample"));
        }
        var report = PulseSurveyEntryReport.BuildPulseSurveyEntryReport("test", items, clock);
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
        PulseSurveyEntryReportCountsEachRecordOnce();
    }
}
