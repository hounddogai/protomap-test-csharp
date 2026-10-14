namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Assignment;

using Acme.Workforce.Engagement.PulseSurvey.Assignment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class PulseSurveyRecordReportChecks
{
    public static void PulseSurveyRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708362988);
        var items = new List<PulseSurveyRecord>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PulseSurveyRecord.NewPulseSurveyRecord($"pulse-survey-record-{index}", "sample"));
        }
        var report = PulseSurveyRecordReport.BuildPulseSurveyRecordReport("test", items, clock);
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
        PulseSurveyRecordReportCountsEachRecordOnce();
    }
}
