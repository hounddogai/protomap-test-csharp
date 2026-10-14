namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Group;

using Acme.Workforce.Engagement.PulseSurvey.Group;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PulseSurveyAdjustmentReportChecks
{
    public static void PulseSurveyAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701346460);
        var items = new List<PulseSurveyAdjustment>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PulseSurveyAdjustment.NewPulseSurveyAdjustment($"pulse-survey-adjustment-{index}", "sample"));
        }
        var report = PulseSurveyAdjustmentReport.BuildPulseSurveyAdjustmentReport("test", items, clock);
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
        PulseSurveyAdjustmentReportCountsEachRecordOnce();
    }
}
