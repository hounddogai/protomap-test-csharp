namespace Acme.Workforce.Contractors.Tests.Vendor.Segment;

using Acme.Workforce.Contractors.Vendor.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class VendorQuotaChecks
{
    public static void VendorQuotaStartsInProposed()
    {
        var item = VendorQuota.NewVendorQuota("code-599", "title-948");
        Check.Equal(item.Status, VendorQuotaStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VendorQuotaMovesToApproved()
    {
        var item = VendorQuota.NewVendorQuota("code-599", "title-948");
        try
        {
            item.MoveTo(VendorQuotaStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VendorQuotaStatus.Approved, "status");
    }

    public static void VendorQuotaRejectsMovesFromWithdrawn()
    {
        var item = VendorQuota.NewVendorQuota("code-599", "title-948");
        item.Status = VendorQuotaStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(VendorQuotaStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        VendorQuotaStartsInProposed();
        VendorQuotaMovesToApproved();
        VendorQuotaRejectsMovesFromWithdrawn();
    }
}
