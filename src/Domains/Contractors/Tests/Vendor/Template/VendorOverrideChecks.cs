namespace Acme.Workforce.Contractors.Tests.Vendor.Template;

using Acme.Workforce.Contractors.Vendor.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class VendorOverrideChecks
{
    public static void VendorOverrideStartsInIdle()
    {
        var item = VendorOverride.NewVendorOverride("code-841", "label-377");
        Check.Equal(item.Status, VendorOverrideStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VendorOverrideMovesToWarming()
    {
        var item = VendorOverride.NewVendorOverride("code-841", "label-377");
        try
        {
            item.MoveTo(VendorOverrideStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VendorOverrideStatus.Warming, "status");
    }

    public static void VendorOverrideRejectsMovesFromStopped()
    {
        var item = VendorOverride.NewVendorOverride("code-841", "label-377");
        item.Status = VendorOverrideStatus.Stopped;
        Check.Fails(() => item.MoveTo(VendorOverrideStatus.Idle), "move");
    }

    public static void RunAll()
    {
        VendorOverrideStartsInIdle();
        VendorOverrideMovesToWarming();
        VendorOverrideRejectsMovesFromStopped();
    }
}
