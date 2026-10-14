namespace Acme.Workforce.Payrollops.Tests.TaxForm.Ledger;

using Acme.Workforce.Payrollops.TaxForm.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class TaxFormRequestChecks
{
    public static void TaxFormRequestStartsInNew()
    {
        var item = TaxFormRequest.NewTaxFormRequest("code-346", "label-332");
        Check.Equal(item.Status, TaxFormRequestStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormRequestMovesToRunning()
    {
        var item = TaxFormRequest.NewTaxFormRequest("code-346", "label-332");
        try
        {
            item.MoveTo(TaxFormRequestStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormRequestStatus.Running, "status");
    }

    public static void TaxFormRequestRejectsMovesFromFailed()
    {
        var item = TaxFormRequest.NewTaxFormRequest("code-346", "label-332");
        item.Status = TaxFormRequestStatus.Failed;
        Check.Fails(() => item.MoveTo(TaxFormRequestStatus.New), "move");
    }

    public static void RunAll()
    {
        TaxFormRequestStartsInNew();
        TaxFormRequestMovesToRunning();
        TaxFormRequestRejectsMovesFromFailed();
    }
}
