namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackProposal;

using Acme.Workforce.Engagement.Feedback.FeedbackProposal;

/// <summary>Checks of feedback proposal code, run by the project's runner.</summary>
public static class FeedbackEntryChecks
{
    public static void FeedbackEntryStartsInDraft()
    {
        var item = FeedbackEntry.NewFeedbackEntry("code-51", "label-215");
        Check.Equal(item.Status, FeedbackEntryStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackEntryMovesToActive()
    {
        var item = FeedbackEntry.NewFeedbackEntry("code-51", "label-215");
        try
        {
            item.MoveTo(FeedbackEntryStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackEntryStatus.Active, "status");
    }

    public static void FeedbackEntryRejectsMovesFromClosed()
    {
        var item = FeedbackEntry.NewFeedbackEntry("code-51", "label-215");
        item.Status = FeedbackEntryStatus.Closed;
        Check.Fails(() => item.MoveTo(FeedbackEntryStatus.Draft), "move");
    }

    public static void RunAll()
    {
        FeedbackEntryStartsInDraft();
        FeedbackEntryMovesToActive();
        FeedbackEntryRejectsMovesFromClosed();
    }
}
