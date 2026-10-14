namespace Acme.Workforce.Engagement.Tests.Feedback.RecordItem;

using Acme.Workforce.Engagement.Feedback.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class FeedbackScoreChecks
{
    public static void FeedbackScoreStartsInDraft()
    {
        var item = FeedbackScore.NewFeedbackScore("code-36", "name-110");
        Check.Equal(item.Status, FeedbackScoreStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackScoreMovesToActive()
    {
        var item = FeedbackScore.NewFeedbackScore("code-36", "name-110");
        try
        {
            item.MoveTo(FeedbackScoreStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackScoreStatus.Active, "status");
    }

    public static void FeedbackScoreRejectsMovesFromClosed()
    {
        var item = FeedbackScore.NewFeedbackScore("code-36", "name-110");
        item.Status = FeedbackScoreStatus.Closed;
        Check.Fails(() => item.MoveTo(FeedbackScoreStatus.Draft), "move");
    }

    public static void RunAll()
    {
        FeedbackScoreStartsInDraft();
        FeedbackScoreMovesToActive();
        FeedbackScoreRejectsMovesFromClosed();
    }
}
