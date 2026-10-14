namespace Acme.Workforce.Payrollops.Tests.TaxForm.Score;

using Acme.Workforce.Payrollops.TaxForm.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class LenientTaxFormEstimateChecks
{
    public static void LenientTaxFormEstimateStartsInOpen()
    {
        var item = LenientTaxFormEstimate.NewLenientTaxFormEstimate("code-446", "label-118");
        Check.Equal(item.Status, LenientTaxFormEstimateStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientTaxFormEstimateMovesToResolved()
    {
        var item = LenientTaxFormEstimate.NewLenientTaxFormEstimate("code-446", "label-118");
        try
        {
            item.MoveTo(LenientTaxFormEstimateStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientTaxFormEstimateStatus.Resolved, "status");
    }

    public static void LenientTaxFormEstimateRejectsMovesFromArchived()
    {
        var item = LenientTaxFormEstimate.NewLenientTaxFormEstimate("code-446", "label-118");
        item.Status = LenientTaxFormEstimateStatus.Archived;
        Check.Fails(() => item.MoveTo(LenientTaxFormEstimateStatus.Open), "move");
    }

    public static void RunAll()
    {
        LenientTaxFormEstimateStartsInOpen();
        LenientTaxFormEstimateMovesToResolved();
        LenientTaxFormEstimateRejectsMovesFromArchived();
    }
}
