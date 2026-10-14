namespace Acme.Workforce.Engagement.Tests.Feedback.Audit;

using Acme.Workforce.Engagement.Feedback.Audit;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class AdaptiveFeedbackPreferenceReportChecks
{
    public static void AdaptiveFeedbackPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704612315);
        var items = new List<AdaptiveFeedbackPreference>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AdaptiveFeedbackPreference.NewAdaptiveFeedbackPreference($"adaptive-feedback-preference-{index}", "sample"));
        }
        var report = AdaptiveFeedbackPreferenceReport.BuildAdaptiveFeedbackPreferenceReport("test", items, clock);
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
        AdaptiveFeedbackPreferenceReportCountsEachRecordOnce();
    }
}
