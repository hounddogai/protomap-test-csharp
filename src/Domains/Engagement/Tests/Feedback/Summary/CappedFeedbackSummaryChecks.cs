namespace Acme.Workforce.Engagement.Tests.Feedback.Summary;

using Acme.Workforce.Engagement.Feedback.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class CappedFeedbackSummaryChecks
{
    public static void CappedFeedbackSummaryStartsInProposed()
    {
        var item = CappedFeedbackSummary.NewCappedFeedbackSummary("code-527", "label-425");
        Check.Equal(item.Status, CappedFeedbackSummaryStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedFeedbackSummaryMovesToRejected()
    {
        var item = CappedFeedbackSummary.NewCappedFeedbackSummary("code-527", "label-425");
        try
        {
            item.MoveTo(CappedFeedbackSummaryStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedFeedbackSummaryStatus.Rejected, "status");
    }

    public static void CappedFeedbackSummaryRejectsMovesFromWithdrawn()
    {
        var item = CappedFeedbackSummary.NewCappedFeedbackSummary("code-527", "label-425");
        item.Status = CappedFeedbackSummaryStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(CappedFeedbackSummaryStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        CappedFeedbackSummaryStartsInProposed();
        CappedFeedbackSummaryMovesToRejected();
        CappedFeedbackSummaryRejectsMovesFromWithdrawn();
    }
}
