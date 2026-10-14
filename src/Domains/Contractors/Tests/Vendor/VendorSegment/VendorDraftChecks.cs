namespace Acme.Workforce.Contractors.Tests.Vendor.VendorSegment;

using Acme.Workforce.Contractors.Vendor.VendorSegment;

/// <summary>Checks of vendor segment code, run by the project's runner.</summary>
public static class VendorDraftChecks
{
    public static void VendorDraftStartsInProposed()
    {
        var item = VendorDraft.NewVendorDraft("code-548", "title-748");
        Check.Equal(item.Status, VendorDraftStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VendorDraftMovesToApproved()
    {
        var item = VendorDraft.NewVendorDraft("code-548", "title-748");
        try
        {
            item.MoveTo(VendorDraftStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VendorDraftStatus.Approved, "status");
    }

    public static void VendorDraftRejectsMovesFromWithdrawn()
    {
        var item = VendorDraft.NewVendorDraft("code-548", "title-748");
        item.Status = VendorDraftStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(VendorDraftStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        VendorDraftStartsInProposed();
        VendorDraftMovesToApproved();
        VendorDraftRejectsMovesFromWithdrawn();
    }
}
