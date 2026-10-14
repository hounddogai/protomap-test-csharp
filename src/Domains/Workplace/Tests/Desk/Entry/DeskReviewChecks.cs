namespace Acme.Workforce.Workplace.Tests.Desk.Entry;

using Acme.Workforce.Workplace.Desk.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class DeskReviewChecks
{
    public static void DeskReviewStartsInPlanned()
    {
        var item = DeskReview.NewDeskReview("code-550", "name-414");
        Check.Equal(item.Status, DeskReviewStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskReviewMovesToInProgress()
    {
        var item = DeskReview.NewDeskReview("code-550", "name-414");
        try
        {
            item.MoveTo(DeskReviewStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskReviewStatus.InProgress, "status");
    }

    public static void DeskReviewRejectsMovesFromCancelled()
    {
        var item = DeskReview.NewDeskReview("code-550", "name-414");
        item.Status = DeskReviewStatus.Cancelled;
        Check.Fails(() => item.MoveTo(DeskReviewStatus.Planned), "move");
    }

    public static void RunAll()
    {
        DeskReviewStartsInPlanned();
        DeskReviewMovesToInProgress();
        DeskReviewRejectsMovesFromCancelled();
    }
}
