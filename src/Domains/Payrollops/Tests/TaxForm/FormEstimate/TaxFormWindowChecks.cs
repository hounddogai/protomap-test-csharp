namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormEstimate;

using Acme.Workforce.Payrollops.TaxForm.FormEstimate;

/// <summary>Checks of form estimate code, run by the project's runner.</summary>
public static class TaxFormWindowChecks
{
    public static void TaxFormWindowStartsInPlanned()
    {
        var item = TaxFormWindow.NewTaxFormWindow("code-250", "name-515");
        Check.Equal(item.Status, TaxFormWindowStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormWindowMovesToInProgress()
    {
        var item = TaxFormWindow.NewTaxFormWindow("code-250", "name-515");
        try
        {
            item.MoveTo(TaxFormWindowStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormWindowStatus.InProgress, "status");
    }

    public static void TaxFormWindowRejectsMovesFromCancelled()
    {
        var item = TaxFormWindow.NewTaxFormWindow("code-250", "name-515");
        item.Status = TaxFormWindowStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TaxFormWindowStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TaxFormWindowStartsInPlanned();
        TaxFormWindowMovesToInProgress();
        TaxFormWindowRejectsMovesFromCancelled();
    }
}
