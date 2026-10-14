namespace Acme.Workforce.Payrollops.Tests.TaxForm.Entry;

using Acme.Workforce.Payrollops.TaxForm.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class TaxFormThresholdChecks
{
    public static void TaxFormThresholdStartsInDraft()
    {
        var item = TaxFormThreshold.NewTaxFormThreshold("code-378", "label-67");
        Check.Equal(item.Status, TaxFormThresholdStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormThresholdMovesToPending()
    {
        var item = TaxFormThreshold.NewTaxFormThreshold("code-378", "label-67");
        try
        {
            item.MoveTo(TaxFormThresholdStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormThresholdStatus.Pending, "status");
    }

    public static void TaxFormThresholdRejectsMovesFromClosed()
    {
        var item = TaxFormThreshold.NewTaxFormThreshold("code-378", "label-67");
        item.Status = TaxFormThresholdStatus.Closed;
        Check.Fails(() => item.MoveTo(TaxFormThresholdStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TaxFormThresholdStartsInDraft();
        TaxFormThresholdMovesToPending();
        TaxFormThresholdRejectsMovesFromClosed();
    }
}
