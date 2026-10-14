namespace Acme.Workforce.Payrollops.Tests.TaxForm.Allocation;

using Acme.Workforce.Payrollops.TaxForm.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class TaxFormAdjustmentChecks
{
    public static void TaxFormAdjustmentStartsInOpen()
    {
        var item = TaxFormAdjustment.NewTaxFormAdjustment("code-803", "label-588");
        Check.Equal(item.Status, TaxFormAdjustmentStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormAdjustmentMovesToReopened()
    {
        var item = TaxFormAdjustment.NewTaxFormAdjustment("code-803", "label-588");
        try
        {
            item.MoveTo(TaxFormAdjustmentStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormAdjustmentStatus.Reopened, "status");
    }

    public static void TaxFormAdjustmentRejectsMovesFromArchived()
    {
        var item = TaxFormAdjustment.NewTaxFormAdjustment("code-803", "label-588");
        item.Status = TaxFormAdjustmentStatus.Archived;
        Check.Fails(() => item.MoveTo(TaxFormAdjustmentStatus.Open), "move");
    }

    public static void RunAll()
    {
        TaxFormAdjustmentStartsInOpen();
        TaxFormAdjustmentMovesToReopened();
        TaxFormAdjustmentRejectsMovesFromArchived();
    }
}
