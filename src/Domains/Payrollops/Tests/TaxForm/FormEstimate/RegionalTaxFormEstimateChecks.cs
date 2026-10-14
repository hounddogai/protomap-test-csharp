namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormEstimate;

using Acme.Workforce.Payrollops.TaxForm.FormEstimate;

/// <summary>Checks of form estimate code, run by the project's runner.</summary>
public static class RegionalTaxFormEstimateChecks
{
    public static void RegionalTaxFormEstimateStartsInDraft()
    {
        var item = RegionalTaxFormEstimate.NewRegionalTaxFormEstimate("code-349", "label-684");
        Check.Equal(item.Status, RegionalTaxFormEstimateStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalTaxFormEstimateMovesToPending()
    {
        var item = RegionalTaxFormEstimate.NewRegionalTaxFormEstimate("code-349", "label-684");
        try
        {
            item.MoveTo(RegionalTaxFormEstimateStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalTaxFormEstimateStatus.Pending, "status");
    }

    public static void RegionalTaxFormEstimateRejectsMovesFromClosed()
    {
        var item = RegionalTaxFormEstimate.NewRegionalTaxFormEstimate("code-349", "label-684");
        item.Status = RegionalTaxFormEstimateStatus.Closed;
        Check.Fails(() => item.MoveTo(RegionalTaxFormEstimateStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RegionalTaxFormEstimateStartsInDraft();
        RegionalTaxFormEstimateMovesToPending();
        RegionalTaxFormEstimateRejectsMovesFromClosed();
    }
}
