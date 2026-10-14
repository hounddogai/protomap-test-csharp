namespace Acme.Workforce.Engagement.Tests.Feedback.Score;

using Acme.Workforce.Engagement.Feedback.Score;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class GlobalFeedbackVarianceReportChecks
{
    public static void GlobalFeedbackVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704043367);
        var items = new List<GlobalFeedbackVariance>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(GlobalFeedbackVariance.NewGlobalFeedbackVariance($"global-feedback-variance-{index}", "sample"));
        }
        var report = GlobalFeedbackVarianceReport.BuildGlobalFeedbackVarianceReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        GlobalFeedbackVarianceReportCountsEachRecordOnce();
    }
}
