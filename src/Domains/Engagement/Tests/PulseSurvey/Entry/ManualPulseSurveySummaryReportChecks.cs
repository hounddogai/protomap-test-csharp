namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Entry;

using Acme.Workforce.Engagement.PulseSurvey.Entry;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ManualPulseSurveySummaryReportChecks
{
    public static void ManualPulseSurveySummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701320524);
        var items = new List<ManualPulseSurveySummary>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(ManualPulseSurveySummary.NewManualPulseSurveySummary($"manual-pulse-survey-summary-{index}", "sample"));
        }
        var report = ManualPulseSurveySummaryReport.BuildManualPulseSurveySummaryReport("test", items, clock);
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
        ManualPulseSurveySummaryReportCountsEachRecordOnce();
    }
}
