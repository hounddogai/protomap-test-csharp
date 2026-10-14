namespace Acme.Workforce.Engagement.Tests.Feedback.Quota;

using Acme.Workforce.Engagement.Feedback.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class FeedbackNoticeChecks
{
    public static void FeedbackNoticeStartsInDraft()
    {
        var item = FeedbackNotice.NewFeedbackNotice("code-465", "label-478");
        Check.Equal(item.Status, FeedbackNoticeStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackNoticeMovesToPending()
    {
        var item = FeedbackNotice.NewFeedbackNotice("code-465", "label-478");
        try
        {
            item.MoveTo(FeedbackNoticeStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackNoticeStatus.Pending, "status");
    }

    public static void FeedbackNoticeRejectsMovesFromClosed()
    {
        var item = FeedbackNotice.NewFeedbackNotice("code-465", "label-478");
        item.Status = FeedbackNoticeStatus.Closed;
        Check.Fails(() => item.MoveTo(FeedbackNoticeStatus.Draft), "move");
    }

    public static void RunAll()
    {
        FeedbackNoticeStartsInDraft();
        FeedbackNoticeMovesToPending();
        FeedbackNoticeRejectsMovesFromClosed();
    }
}
