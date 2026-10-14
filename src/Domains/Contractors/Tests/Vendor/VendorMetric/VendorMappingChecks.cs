namespace Acme.Workforce.Contractors.Tests.Vendor.VendorMetric;

using Acme.Workforce.Contractors.Vendor.VendorMetric;

/// <summary>Checks of vendor metric code, run by the project's runner.</summary>
public static class VendorMappingChecks
{
    public static void VendorMappingStartsInProposed()
    {
        var item = VendorMapping.NewVendorMapping("code-274", "title-517");
        Check.Equal(item.Status, VendorMappingStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VendorMappingMovesToRejected()
    {
        var item = VendorMapping.NewVendorMapping("code-274", "title-517");
        try
        {
            item.MoveTo(VendorMappingStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VendorMappingStatus.Rejected, "status");
    }

    public static void VendorMappingRejectsMovesFromWithdrawn()
    {
        var item = VendorMapping.NewVendorMapping("code-274", "title-517");
        item.Status = VendorMappingStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(VendorMappingStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        VendorMappingStartsInProposed();
        VendorMappingMovesToRejected();
        VendorMappingRejectsMovesFromWithdrawn();
    }
}
