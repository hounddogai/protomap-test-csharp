namespace Acme.Workforce.Compensation.Tests.PayBand.Score;

using Acme.Workforce.Compensation.PayBand.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class ScheduledPayBandReviewChecks
{
    public static void ScheduledPayBandReviewStartsInDraft()
    {
        var item = ScheduledPayBandReview.NewScheduledPayBandReview("code-914", "title-875");
        Check.Equal(item.Status, ScheduledPayBandReviewStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledPayBandReviewMovesToActive()
    {
        var item = ScheduledPayBandReview.NewScheduledPayBandReview("code-914", "title-875");
        try
        {
            item.MoveTo(ScheduledPayBandReviewStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledPayBandReviewStatus.Active, "status");
    }

    public static void ScheduledPayBandReviewRejectsMovesFromClosed()
    {
        var item = ScheduledPayBandReview.NewScheduledPayBandReview("code-914", "title-875");
        item.Status = ScheduledPayBandReviewStatus.Closed;
        Check.Fails(() => item.MoveTo(ScheduledPayBandReviewStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ScheduledPayBandReviewStartsInDraft();
        ScheduledPayBandReviewMovesToActive();
        ScheduledPayBandReviewRejectsMovesFromClosed();
    }
}
