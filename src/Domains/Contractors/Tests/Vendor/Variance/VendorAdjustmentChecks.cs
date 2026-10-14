namespace Acme.Workforce.Contractors.Tests.Vendor.Variance;

using Acme.Workforce.Contractors.Vendor.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class VendorAdjustmentChecks
{
    public static void VendorAdjustmentStartsInIdle()
    {
        var item = VendorAdjustment.NewVendorAdjustment("code-403", "title-620");
        Check.Equal(item.Status, VendorAdjustmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VendorAdjustmentMovesToReady()
    {
        var item = VendorAdjustment.NewVendorAdjustment("code-403", "title-620");
        try
        {
            item.MoveTo(VendorAdjustmentStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VendorAdjustmentStatus.Ready, "status");
    }

    public static void VendorAdjustmentRejectsMovesFromStopped()
    {
        var item = VendorAdjustment.NewVendorAdjustment("code-403", "title-620");
        item.Status = VendorAdjustmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(VendorAdjustmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        VendorAdjustmentStartsInIdle();
        VendorAdjustmentMovesToReady();
        VendorAdjustmentRejectsMovesFromStopped();
    }
}
