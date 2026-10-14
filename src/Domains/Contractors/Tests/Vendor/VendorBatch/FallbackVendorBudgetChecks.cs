namespace Acme.Workforce.Contractors.Tests.Vendor.VendorBatch;

using Acme.Workforce.Contractors.Vendor.VendorBatch;

/// <summary>Checks of vendor batch code, run by the project's runner.</summary>
public static class FallbackVendorBudgetChecks
{
    public static void FallbackVendorBudgetStartsInPlanned()
    {
        var item = FallbackVendorBudget.NewFallbackVendorBudget("code-306", "title-985");
        Check.Equal(item.Status, FallbackVendorBudgetStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackVendorBudgetMovesToScheduled()
    {
        var item = FallbackVendorBudget.NewFallbackVendorBudget("code-306", "title-985");
        try
        {
            item.MoveTo(FallbackVendorBudgetStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackVendorBudgetStatus.Scheduled, "status");
    }

    public static void FallbackVendorBudgetRejectsMovesFromCancelled()
    {
        var item = FallbackVendorBudget.NewFallbackVendorBudget("code-306", "title-985");
        item.Status = FallbackVendorBudgetStatus.Cancelled;
        Check.Fails(() => item.MoveTo(FallbackVendorBudgetStatus.Planned), "move");
    }

    public static void RunAll()
    {
        FallbackVendorBudgetStartsInPlanned();
        FallbackVendorBudgetMovesToScheduled();
        FallbackVendorBudgetRejectsMovesFromCancelled();
    }
}
