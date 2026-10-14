namespace Acme.Workforce.Payrollops.Tests.Deduction.Review;

using Acme.Workforce.Payrollops.Deduction.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class SeasonalDeductionReviewChecks
{
    public static void SeasonalDeductionReviewStartsInDraft()
    {
        var item = SeasonalDeductionReview.NewSeasonalDeductionReview("code-198", "label-193");
        Check.Equal(item.Status, SeasonalDeductionReviewStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalDeductionReviewMovesToPending()
    {
        var item = SeasonalDeductionReview.NewSeasonalDeductionReview("code-198", "label-193");
        try
        {
            item.MoveTo(SeasonalDeductionReviewStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalDeductionReviewStatus.Pending, "status");
    }

    public static void SeasonalDeductionReviewRejectsMovesFromClosed()
    {
        var item = SeasonalDeductionReview.NewSeasonalDeductionReview("code-198", "label-193");
        item.Status = SeasonalDeductionReviewStatus.Closed;
        Check.Fails(() => item.MoveTo(SeasonalDeductionReviewStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SeasonalDeductionReviewStartsInDraft();
        SeasonalDeductionReviewMovesToPending();
        SeasonalDeductionReviewRejectsMovesFromClosed();
    }
}
