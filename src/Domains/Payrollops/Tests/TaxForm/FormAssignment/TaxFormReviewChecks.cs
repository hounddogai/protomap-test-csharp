namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormAssignment;

using Acme.Workforce.Payrollops.TaxForm.FormAssignment;

/// <summary>Checks of form assignment code, run by the project's runner.</summary>
public static class TaxFormReviewChecks
{
    public static void TaxFormReviewStartsInDraft()
    {
        var item = TaxFormReview.NewTaxFormReview("code-839", "label-497");
        Check.Equal(item.Status, TaxFormReviewStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormReviewMovesToPending()
    {
        var item = TaxFormReview.NewTaxFormReview("code-839", "label-497");
        try
        {
            item.MoveTo(TaxFormReviewStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormReviewStatus.Pending, "status");
    }

    public static void TaxFormReviewRejectsMovesFromClosed()
    {
        var item = TaxFormReview.NewTaxFormReview("code-839", "label-497");
        item.Status = TaxFormReviewStatus.Closed;
        Check.Fails(() => item.MoveTo(TaxFormReviewStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TaxFormReviewStartsInDraft();
        TaxFormReviewMovesToPending();
        TaxFormReviewRejectsMovesFromClosed();
    }
}
