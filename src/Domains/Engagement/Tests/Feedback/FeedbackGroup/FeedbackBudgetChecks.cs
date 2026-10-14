namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackGroup;

using Acme.Workforce.Engagement.Feedback.FeedbackGroup;

/// <summary>Checks of feedback group code, run by the project's runner.</summary>
public static class FeedbackBudgetChecks
{
    public static void FeedbackBudgetStartsInDraft()
    {
        var item = FeedbackBudget.NewFeedbackBudget("code-586", "label-382");
        Check.Equal(item.Status, FeedbackBudgetStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackBudgetMovesToSuspended()
    {
        var item = FeedbackBudget.NewFeedbackBudget("code-586", "label-382");
        try
        {
            item.MoveTo(FeedbackBudgetStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackBudgetStatus.Suspended, "status");
    }

    public static void FeedbackBudgetRejectsMovesFromClosed()
    {
        var item = FeedbackBudget.NewFeedbackBudget("code-586", "label-382");
        item.Status = FeedbackBudgetStatus.Closed;
        Check.Fails(() => item.MoveTo(FeedbackBudgetStatus.Draft), "move");
    }

    public static void RunAll()
    {
        FeedbackBudgetStartsInDraft();
        FeedbackBudgetMovesToSuspended();
        FeedbackBudgetRejectsMovesFromClosed();
    }
}
