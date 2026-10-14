namespace Acme.Workforce.Engagement.Tests.Feedback.Manifest;

using Acme.Workforce.Engagement.Feedback.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class FeedbackLedgerChecks
{
    public static void FeedbackLedgerStartsInCreated()
    {
        var item = FeedbackLedger.NewFeedbackLedger("code-560", "label-163");
        Check.Equal(item.Status, FeedbackLedgerStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackLedgerMovesToValidated()
    {
        var item = FeedbackLedger.NewFeedbackLedger("code-560", "label-163");
        try
        {
            item.MoveTo(FeedbackLedgerStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackLedgerStatus.Validated, "status");
    }

    public static void FeedbackLedgerRejectsMovesFromRetired()
    {
        var item = FeedbackLedger.NewFeedbackLedger("code-560", "label-163");
        item.Status = FeedbackLedgerStatus.Retired;
        Check.Fails(() => item.MoveTo(FeedbackLedgerStatus.Created), "move");
    }

    public static void RunAll()
    {
        FeedbackLedgerStartsInCreated();
        FeedbackLedgerMovesToValidated();
        FeedbackLedgerRejectsMovesFromRetired();
    }
}
