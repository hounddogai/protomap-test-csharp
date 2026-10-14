namespace Acme.Workforce.Engagement.Tests.Feedback.Manifest;

using Acme.Workforce.Engagement.Feedback.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class FeedbackLedgerViewChecks
{
    public static void FeedbackLedgerViewShowsTheStatusInCapitals()
    {
        var item = FeedbackLedger.NewFeedbackLedger("code-560", "label-163");
        var view = FeedbackLedgerView.ToFeedbackLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FeedbackLedgerView.ToFeedbackLedgerViews(new List<FeedbackLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FeedbackLedgerViewShowsTheStatusInCapitals();
    }
}
