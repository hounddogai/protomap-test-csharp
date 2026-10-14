namespace Acme.Workforce.Absence.Tests.LeaveRequest.Ledger;

using Acme.Workforce.Absence.LeaveRequest.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class SeasonalLeaveRequestReviewChecks
{
    public static void SeasonalLeaveRequestReviewStartsInProposed()
    {
        var item = SeasonalLeaveRequestReview.NewSeasonalLeaveRequestReview("code-573", "title-876");
        Check.Equal(item.Status, SeasonalLeaveRequestReviewStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalLeaveRequestReviewMovesToWithdrawn()
    {
        var item = SeasonalLeaveRequestReview.NewSeasonalLeaveRequestReview("code-573", "title-876");
        try
        {
            item.MoveTo(SeasonalLeaveRequestReviewStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalLeaveRequestReviewStatus.Withdrawn, "status");
    }

    public static void SeasonalLeaveRequestReviewRejectsMovesFromWithdrawn()
    {
        var item = SeasonalLeaveRequestReview.NewSeasonalLeaveRequestReview("code-573", "title-876");
        item.Status = SeasonalLeaveRequestReviewStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(SeasonalLeaveRequestReviewStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        SeasonalLeaveRequestReviewStartsInProposed();
        SeasonalLeaveRequestReviewMovesToWithdrawn();
        SeasonalLeaveRequestReviewRejectsMovesFromWithdrawn();
    }
}
