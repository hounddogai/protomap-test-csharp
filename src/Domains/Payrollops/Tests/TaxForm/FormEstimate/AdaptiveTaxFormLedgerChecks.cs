namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormEstimate;

using Acme.Workforce.Payrollops.TaxForm.FormEstimate;

/// <summary>Checks of form estimate code, run by the project's runner.</summary>
public static class AdaptiveTaxFormLedgerChecks
{
    public static void AdaptiveTaxFormLedgerStartsInDraft()
    {
        var item = AdaptiveTaxFormLedger.NewAdaptiveTaxFormLedger("code-782", "label-956");
        Check.Equal(item.Status, AdaptiveTaxFormLedgerStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveTaxFormLedgerMovesToPending()
    {
        var item = AdaptiveTaxFormLedger.NewAdaptiveTaxFormLedger("code-782", "label-956");
        try
        {
            item.MoveTo(AdaptiveTaxFormLedgerStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveTaxFormLedgerStatus.Pending, "status");
    }

    public static void AdaptiveTaxFormLedgerRejectsMovesFromClosed()
    {
        var item = AdaptiveTaxFormLedger.NewAdaptiveTaxFormLedger("code-782", "label-956");
        item.Status = AdaptiveTaxFormLedgerStatus.Closed;
        Check.Fails(() => item.MoveTo(AdaptiveTaxFormLedgerStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AdaptiveTaxFormLedgerStartsInDraft();
        AdaptiveTaxFormLedgerMovesToPending();
        AdaptiveTaxFormLedgerRejectsMovesFromClosed();
    }
}
