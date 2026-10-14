namespace Acme.Workforce.Contractors.Tests.Vendor.Rollup;

using Acme.Workforce.Contractors.Vendor.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class VendorRollupChecks
{
    public static void VendorRollupStartsInIdle()
    {
        var item = VendorRollup.NewVendorRollup("code-713", "title-960");
        Check.Equal(item.Status, VendorRollupStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VendorRollupMovesToDraining()
    {
        var item = VendorRollup.NewVendorRollup("code-713", "title-960");
        try
        {
            item.MoveTo(VendorRollupStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VendorRollupStatus.Draining, "status");
    }

    public static void VendorRollupRejectsMovesFromStopped()
    {
        var item = VendorRollup.NewVendorRollup("code-713", "title-960");
        item.Status = VendorRollupStatus.Stopped;
        Check.Fails(() => item.MoveTo(VendorRollupStatus.Idle), "move");
    }

    public static void RunAll()
    {
        VendorRollupStartsInIdle();
        VendorRollupMovesToDraining();
        VendorRollupRejectsMovesFromStopped();
    }
}
