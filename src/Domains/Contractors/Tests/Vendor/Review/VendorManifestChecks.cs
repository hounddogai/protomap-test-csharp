namespace Acme.Workforce.Contractors.Tests.Vendor.Review;

using Acme.Workforce.Contractors.Vendor.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class VendorManifestChecks
{
    public static void VendorManifestStartsInProposed()
    {
        var item = VendorManifest.NewVendorManifest("code-44", "label-895");
        Check.Equal(item.Status, VendorManifestStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VendorManifestMovesToApproved()
    {
        var item = VendorManifest.NewVendorManifest("code-44", "label-895");
        try
        {
            item.MoveTo(VendorManifestStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VendorManifestStatus.Approved, "status");
    }

    public static void VendorManifestRejectsMovesFromWithdrawn()
    {
        var item = VendorManifest.NewVendorManifest("code-44", "label-895");
        item.Status = VendorManifestStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(VendorManifestStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        VendorManifestStartsInProposed();
        VendorManifestMovesToApproved();
        VendorManifestRejectsMovesFromWithdrawn();
    }
}
