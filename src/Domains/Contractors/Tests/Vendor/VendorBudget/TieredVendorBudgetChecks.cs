namespace Acme.Workforce.Contractors.Tests.Vendor.VendorBudget;

using Acme.Workforce.Contractors.Vendor.VendorBudget;

/// <summary>Checks of vendor budget code, run by the project's runner.</summary>
public static class TieredVendorBudgetChecks
{
    public static void TieredVendorBudgetStartsInDraft()
    {
        var item = TieredVendorBudget.NewTieredVendorBudget("code-571", "label-228");
        Check.Equal(item.Status, TieredVendorBudgetStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredVendorBudgetMovesToPending()
    {
        var item = TieredVendorBudget.NewTieredVendorBudget("code-571", "label-228");
        try
        {
            item.MoveTo(TieredVendorBudgetStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredVendorBudgetStatus.Pending, "status");
    }

    public static void TieredVendorBudgetRejectsMovesFromClosed()
    {
        var item = TieredVendorBudget.NewTieredVendorBudget("code-571", "label-228");
        item.Status = TieredVendorBudgetStatus.Closed;
        Check.Fails(() => item.MoveTo(TieredVendorBudgetStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TieredVendorBudgetStartsInDraft();
        TieredVendorBudgetMovesToPending();
        TieredVendorBudgetRejectsMovesFromClosed();
    }
}
