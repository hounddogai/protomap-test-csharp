namespace Acme.Workforce.Payrollops.Tests.TaxForm.Ledger;

using Acme.Workforce.Payrollops.TaxForm.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class ManualTaxFormCycleChecks
{
    public static void ManualTaxFormCycleStartsInProposed()
    {
        var item = ManualTaxFormCycle.NewManualTaxFormCycle("code-902", "name-723");
        Check.Equal(item.Status, ManualTaxFormCycleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualTaxFormCycleMovesToApproved()
    {
        var item = ManualTaxFormCycle.NewManualTaxFormCycle("code-902", "name-723");
        try
        {
            item.MoveTo(ManualTaxFormCycleStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualTaxFormCycleStatus.Approved, "status");
    }

    public static void ManualTaxFormCycleRejectsMovesFromWithdrawn()
    {
        var item = ManualTaxFormCycle.NewManualTaxFormCycle("code-902", "name-723");
        item.Status = ManualTaxFormCycleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ManualTaxFormCycleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ManualTaxFormCycleStartsInProposed();
        ManualTaxFormCycleMovesToApproved();
        ManualTaxFormCycleRejectsMovesFromWithdrawn();
    }
}
