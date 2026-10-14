namespace Acme.Workforce.Payrollops.Tests.TaxForm.Allocation;

using Acme.Workforce.Payrollops.TaxForm.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class RollingTaxFormAdjustmentChecks
{
    public static void RollingTaxFormAdjustmentStartsInIdle()
    {
        var item = RollingTaxFormAdjustment.NewRollingTaxFormAdjustment("code-971", "name-158");
        Check.Equal(item.Status, RollingTaxFormAdjustmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingTaxFormAdjustmentMovesToReady()
    {
        var item = RollingTaxFormAdjustment.NewRollingTaxFormAdjustment("code-971", "name-158");
        try
        {
            item.MoveTo(RollingTaxFormAdjustmentStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingTaxFormAdjustmentStatus.Ready, "status");
    }

    public static void RollingTaxFormAdjustmentRejectsMovesFromStopped()
    {
        var item = RollingTaxFormAdjustment.NewRollingTaxFormAdjustment("code-971", "name-158");
        item.Status = RollingTaxFormAdjustmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(RollingTaxFormAdjustmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RollingTaxFormAdjustmentStartsInIdle();
        RollingTaxFormAdjustmentMovesToReady();
        RollingTaxFormAdjustmentRejectsMovesFromStopped();
    }
}
