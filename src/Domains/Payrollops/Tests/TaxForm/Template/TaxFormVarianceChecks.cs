namespace Acme.Workforce.Payrollops.Tests.TaxForm.Template;

using Acme.Workforce.Payrollops.TaxForm.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class TaxFormVarianceChecks
{
    public static void TaxFormVarianceStartsInPlanned()
    {
        var item = TaxFormVariance.NewTaxFormVariance("code-21", "label-217");
        Check.Equal(item.Status, TaxFormVarianceStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormVarianceMovesToScheduled()
    {
        var item = TaxFormVariance.NewTaxFormVariance("code-21", "label-217");
        try
        {
            item.MoveTo(TaxFormVarianceStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormVarianceStatus.Scheduled, "status");
    }

    public static void TaxFormVarianceRejectsMovesFromCancelled()
    {
        var item = TaxFormVariance.NewTaxFormVariance("code-21", "label-217");
        item.Status = TaxFormVarianceStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TaxFormVarianceStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TaxFormVarianceStartsInPlanned();
        TaxFormVarianceMovesToScheduled();
        TaxFormVarianceRejectsMovesFromCancelled();
    }
}
