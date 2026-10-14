namespace Acme.Workforce.Engagement.Tests.Feedback.Notice;

using Acme.Workforce.Engagement.Feedback.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class PrimaryFeedbackNoticeChecks
{
    public static void PrimaryFeedbackNoticeStartsInDraft()
    {
        var item = PrimaryFeedbackNotice.NewPrimaryFeedbackNotice("code-100", "title-935");
        Check.Equal(item.Status, PrimaryFeedbackNoticeStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryFeedbackNoticeMovesToPending()
    {
        var item = PrimaryFeedbackNotice.NewPrimaryFeedbackNotice("code-100", "title-935");
        try
        {
            item.MoveTo(PrimaryFeedbackNoticeStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryFeedbackNoticeStatus.Pending, "status");
    }

    public static void PrimaryFeedbackNoticeRejectsMovesFromClosed()
    {
        var item = PrimaryFeedbackNotice.NewPrimaryFeedbackNotice("code-100", "title-935");
        item.Status = PrimaryFeedbackNoticeStatus.Closed;
        Check.Fails(() => item.MoveTo(PrimaryFeedbackNoticeStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PrimaryFeedbackNoticeStartsInDraft();
        PrimaryFeedbackNoticeMovesToPending();
        PrimaryFeedbackNoticeRejectsMovesFromClosed();
    }
}
