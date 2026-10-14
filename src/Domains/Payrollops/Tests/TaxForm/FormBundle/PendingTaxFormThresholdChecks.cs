namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormBundle;

using Acme.Workforce.Payrollops.TaxForm.FormBundle;

/// <summary>Checks of form bundle code, run by the project's runner.</summary>
public static class PendingTaxFormThresholdChecks
{
    public static void PendingTaxFormThresholdStartsInNew()
    {
        var item = PendingTaxFormThreshold.NewPendingTaxFormThreshold("code-390", "label-516");
        Check.Equal(item.Status, PendingTaxFormThresholdStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingTaxFormThresholdMovesToDone()
    {
        var item = PendingTaxFormThreshold.NewPendingTaxFormThreshold("code-390", "label-516");
        try
        {
            item.MoveTo(PendingTaxFormThresholdStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingTaxFormThresholdStatus.Done, "status");
    }

    public static void PendingTaxFormThresholdRejectsMovesFromFailed()
    {
        var item = PendingTaxFormThreshold.NewPendingTaxFormThreshold("code-390", "label-516");
        item.Status = PendingTaxFormThresholdStatus.Failed;
        Check.Fails(() => item.MoveTo(PendingTaxFormThresholdStatus.New), "move");
    }

    public static void RunAll()
    {
        PendingTaxFormThresholdStartsInNew();
        PendingTaxFormThresholdMovesToDone();
        PendingTaxFormThresholdRejectsMovesFromFailed();
    }
}
