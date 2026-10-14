namespace Acme.Workforce.Engagement.Tests.TownHall.Rule;

using Acme.Workforce.Engagement.TownHall.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class TownHallReviewChecks
{
    public static void TownHallReviewStartsInDraft()
    {
        var item = TownHallReview.NewTownHallReview("code-757", "label-498");
        Check.Equal(item.Status, TownHallReviewStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallReviewMovesToPending()
    {
        var item = TownHallReview.NewTownHallReview("code-757", "label-498");
        try
        {
            item.MoveTo(TownHallReviewStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallReviewStatus.Pending, "status");
    }

    public static void TownHallReviewRejectsMovesFromClosed()
    {
        var item = TownHallReview.NewTownHallReview("code-757", "label-498");
        item.Status = TownHallReviewStatus.Closed;
        Check.Fails(() => item.MoveTo(TownHallReviewStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TownHallReviewStartsInDraft();
        TownHallReviewMovesToPending();
        TownHallReviewRejectsMovesFromClosed();
    }
}
