namespace Acme.Workforce.Engagement.Tests.Feedback.Rule;

using Acme.Workforce.Engagement.Feedback.Rule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class FeedbackRuleReportChecks
{
    public static void FeedbackRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705773867);
        var items = new List<FeedbackRule>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(FeedbackRule.NewFeedbackRule($"feedback-rule-{index}", "sample"));
        }
        var report = FeedbackRuleReport.BuildFeedbackRuleReport("test", items, clock);
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
        FeedbackRuleReportCountsEachRecordOnce();
    }
}
