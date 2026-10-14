namespace Acme.Workforce.Contractors.Tests.Vendor.Budget;

using Acme.Workforce.Contractors.Vendor.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class VendorBudgetChecks
{
    public static void VendorBudgetStartsInPlanned()
    {
        var item = VendorBudget.NewVendorBudget("code-575", "name-607");
        Check.Equal(item.Status, VendorBudgetStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VendorBudgetMovesToCompleted()
    {
        var item = VendorBudget.NewVendorBudget("code-575", "name-607");
        try
        {
            item.MoveTo(VendorBudgetStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VendorBudgetStatus.Completed, "status");
    }

    public static void VendorBudgetRejectsMovesFromCancelled()
    {
        var item = VendorBudget.NewVendorBudget("code-575", "name-607");
        item.Status = VendorBudgetStatus.Cancelled;
        Check.Fails(() => item.MoveTo(VendorBudgetStatus.Planned), "move");
    }

    public static void RunAll()
    {
        VendorBudgetStartsInPlanned();
        VendorBudgetMovesToCompleted();
        VendorBudgetRejectsMovesFromCancelled();
    }
}
