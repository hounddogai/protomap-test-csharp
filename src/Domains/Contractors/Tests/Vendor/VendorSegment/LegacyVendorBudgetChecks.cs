namespace Acme.Workforce.Contractors.Tests.Vendor.VendorSegment;

using Acme.Workforce.Contractors.Vendor.VendorSegment;

/// <summary>Checks of vendor segment code, run by the project's runner.</summary>
public static class LegacyVendorBudgetChecks
{
    public static void LegacyVendorBudgetStartsInIdle()
    {
        var item = LegacyVendorBudget.NewLegacyVendorBudget("code-198", "name-583");
        Check.Equal(item.Status, LegacyVendorBudgetStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyVendorBudgetMovesToStopped()
    {
        var item = LegacyVendorBudget.NewLegacyVendorBudget("code-198", "name-583");
        try
        {
            item.MoveTo(LegacyVendorBudgetStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyVendorBudgetStatus.Stopped, "status");
    }

    public static void LegacyVendorBudgetRejectsMovesFromStopped()
    {
        var item = LegacyVendorBudget.NewLegacyVendorBudget("code-198", "name-583");
        item.Status = LegacyVendorBudgetStatus.Stopped;
        Check.Fails(() => item.MoveTo(LegacyVendorBudgetStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LegacyVendorBudgetStartsInIdle();
        LegacyVendorBudgetMovesToStopped();
        LegacyVendorBudgetRejectsMovesFromStopped();
    }
}
