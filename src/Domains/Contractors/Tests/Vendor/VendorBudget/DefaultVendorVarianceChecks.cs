namespace Acme.Workforce.Contractors.Tests.Vendor.VendorBudget;

using Acme.Workforce.Contractors.Vendor.VendorBudget;

/// <summary>Checks of vendor budget code, run by the project's runner.</summary>
public static class DefaultVendorVarianceChecks
{
    public static void DefaultVendorVarianceStartsInDraft()
    {
        var item = DefaultVendorVariance.NewDefaultVendorVariance("code-742", "title-510");
        Check.Equal(item.Status, DefaultVendorVarianceStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultVendorVarianceMovesToPending()
    {
        var item = DefaultVendorVariance.NewDefaultVendorVariance("code-742", "title-510");
        try
        {
            item.MoveTo(DefaultVendorVarianceStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultVendorVarianceStatus.Pending, "status");
    }

    public static void DefaultVendorVarianceRejectsMovesFromClosed()
    {
        var item = DefaultVendorVariance.NewDefaultVendorVariance("code-742", "title-510");
        item.Status = DefaultVendorVarianceStatus.Closed;
        Check.Fails(() => item.MoveTo(DefaultVendorVarianceStatus.Draft), "move");
    }

    public static void RunAll()
    {
        DefaultVendorVarianceStartsInDraft();
        DefaultVendorVarianceMovesToPending();
        DefaultVendorVarianceRejectsMovesFromClosed();
    }
}
