namespace Acme.Workforce.Payrollops.Tests.Deduction.Schedule;

using Acme.Workforce.Payrollops.Deduction.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class DeductionReviewChecks
{
    public static void DeductionReviewStartsInOpen()
    {
        var item = DeductionReview.NewDeductionReview("code-26", "label-973");
        Check.Equal(item.Status, DeductionReviewStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionReviewMovesToInReview()
    {
        var item = DeductionReview.NewDeductionReview("code-26", "label-973");
        try
        {
            item.MoveTo(DeductionReviewStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionReviewStatus.InReview, "status");
    }

    public static void DeductionReviewRejectsMovesFromArchived()
    {
        var item = DeductionReview.NewDeductionReview("code-26", "label-973");
        item.Status = DeductionReviewStatus.Archived;
        Check.Fails(() => item.MoveTo(DeductionReviewStatus.Open), "move");
    }

    public static void RunAll()
    {
        DeductionReviewStartsInOpen();
        DeductionReviewMovesToInReview();
        DeductionReviewRejectsMovesFromArchived();
    }
}
