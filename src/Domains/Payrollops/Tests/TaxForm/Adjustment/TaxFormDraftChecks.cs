namespace Acme.Workforce.Payrollops.Tests.TaxForm.Adjustment;

using Acme.Workforce.Payrollops.TaxForm.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class TaxFormDraftChecks
{
    public static void TaxFormDraftStartsInDraft()
    {
        var item = TaxFormDraft.NewTaxFormDraft("code-297", "name-423");
        Check.Equal(item.Status, TaxFormDraftStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormDraftMovesToClosed()
    {
        var item = TaxFormDraft.NewTaxFormDraft("code-297", "name-423");
        try
        {
            item.MoveTo(TaxFormDraftStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormDraftStatus.Closed, "status");
    }

    public static void TaxFormDraftRejectsMovesFromClosed()
    {
        var item = TaxFormDraft.NewTaxFormDraft("code-297", "name-423");
        item.Status = TaxFormDraftStatus.Closed;
        Check.Fails(() => item.MoveTo(TaxFormDraftStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TaxFormDraftStartsInDraft();
        TaxFormDraftMovesToClosed();
        TaxFormDraftRejectsMovesFromClosed();
    }
}
