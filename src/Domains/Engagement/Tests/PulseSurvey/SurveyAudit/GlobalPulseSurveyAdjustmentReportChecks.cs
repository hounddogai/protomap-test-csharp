namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyAudit;

using Acme.Workforce.Engagement.PulseSurvey.SurveyAudit;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of survey audit code, run by the project's runner.</summary>
public static class GlobalPulseSurveyAdjustmentReportChecks
{
    public static void GlobalPulseSurveyAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700560406);
        var items = new List<GlobalPulseSurveyAdjustment>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(GlobalPulseSurveyAdjustment.NewGlobalPulseSurveyAdjustment($"global-pulse-survey-adjustment-{index}", "sample"));
        }
        var report = GlobalPulseSurveyAdjustmentReport.BuildGlobalPulseSurveyAdjustmentReport("test", items, clock);
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
        GlobalPulseSurveyAdjustmentReportCountsEachRecordOnce();
    }
}
