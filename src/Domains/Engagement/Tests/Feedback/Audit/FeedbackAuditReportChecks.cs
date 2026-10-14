namespace Acme.Workforce.Engagement.Tests.Feedback.Audit;

using Acme.Workforce.Engagement.Feedback.Audit;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FeedbackAuditReportChecks
{
    public static void FeedbackAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702261324);
        var items = new List<FeedbackAudit>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(FeedbackAudit.NewFeedbackAudit($"feedback-audit-{index}", "sample"));
        }
        var report = FeedbackAuditReport.BuildFeedbackAuditReport("test", items, clock);
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
        FeedbackAuditReportCountsEachRecordOnce();
    }
}
