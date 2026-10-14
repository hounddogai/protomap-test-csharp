namespace Acme.Workforce.Payrollops.Tests.TaxForm.Adjustment;

using Acme.Workforce.Payrollops.TaxForm.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class RegionalTaxFormAdjustmentChecks
{
    public static void RegionalTaxFormAdjustmentStartsInIdle()
    {
        var item = RegionalTaxFormAdjustment.NewRegionalTaxFormAdjustment("code-137", "title-83");
        Check.Equal(item.Status, RegionalTaxFormAdjustmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalTaxFormAdjustmentMovesToReady()
    {
        var item = RegionalTaxFormAdjustment.NewRegionalTaxFormAdjustment("code-137", "title-83");
        try
        {
            item.MoveTo(RegionalTaxFormAdjustmentStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalTaxFormAdjustmentStatus.Ready, "status");
    }

    public static void RegionalTaxFormAdjustmentRejectsMovesFromStopped()
    {
        var item = RegionalTaxFormAdjustment.NewRegionalTaxFormAdjustment("code-137", "title-83");
        item.Status = RegionalTaxFormAdjustmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(RegionalTaxFormAdjustmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RegionalTaxFormAdjustmentStartsInIdle();
        RegionalTaxFormAdjustmentMovesToReady();
        RegionalTaxFormAdjustmentRejectsMovesFromStopped();
    }
}
