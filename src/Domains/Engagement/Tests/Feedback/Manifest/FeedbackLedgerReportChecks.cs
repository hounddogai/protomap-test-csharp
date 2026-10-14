namespace Acme.Workforce.Engagement.Tests.Feedback.Manifest;

using Acme.Workforce.Engagement.Feedback.Manifest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class FeedbackLedgerReportChecks
{
    public static void FeedbackLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705388237);
        var items = new List<FeedbackLedger>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(FeedbackLedger.NewFeedbackLedger($"feedback-ledger-{index}", "sample"));
        }
        var report = FeedbackLedgerReport.BuildFeedbackLedgerReport("test", items, clock);
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
        FeedbackLedgerReportCountsEachRecordOnce();
    }
}
