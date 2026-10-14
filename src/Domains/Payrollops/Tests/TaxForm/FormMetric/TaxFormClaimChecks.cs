namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormMetric;

using Acme.Workforce.Payrollops.TaxForm.FormMetric;

/// <summary>Checks of form metric code, run by the project's runner.</summary>
public static class TaxFormClaimChecks
{
    public static void TaxFormClaimStartsInDraft()
    {
        var item = TaxFormClaim.NewTaxFormClaim("code-460", "name-312");
        Check.Equal(item.Status, TaxFormClaimStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormClaimMovesToPending()
    {
        var item = TaxFormClaim.NewTaxFormClaim("code-460", "name-312");
        try
        {
            item.MoveTo(TaxFormClaimStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormClaimStatus.Pending, "status");
    }

    public static void TaxFormClaimRejectsMovesFromClosed()
    {
        var item = TaxFormClaim.NewTaxFormClaim("code-460", "name-312");
        item.Status = TaxFormClaimStatus.Closed;
        Check.Fails(() => item.MoveTo(TaxFormClaimStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TaxFormClaimStartsInDraft();
        TaxFormClaimMovesToPending();
        TaxFormClaimRejectsMovesFromClosed();
    }
}
