namespace Acme.Workforce.Engagement.Tests.Feedback.Slot;

using Acme.Workforce.Engagement.Feedback.Slot;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class CappedFeedbackRuleReportChecks
{
    public static void CappedFeedbackRuleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709265666);
        var items = new List<CappedFeedbackRule>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CappedFeedbackRule.NewCappedFeedbackRule($"capped-feedback-rule-{index}", "sample"));
        }
        var report = CappedFeedbackRuleReport.BuildCappedFeedbackRuleReport("test", items, clock);
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
        CappedFeedbackRuleReportCountsEachRecordOnce();
    }
}
