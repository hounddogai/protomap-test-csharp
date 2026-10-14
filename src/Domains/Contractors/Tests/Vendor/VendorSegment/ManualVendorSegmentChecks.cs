namespace Acme.Workforce.Contractors.Tests.Vendor.VendorSegment;

using Acme.Workforce.Contractors.Vendor.VendorSegment;

/// <summary>Checks of vendor segment code, run by the project's runner.</summary>
public static class ManualVendorSegmentChecks
{
    public static void ManualVendorSegmentStartsInProposed()
    {
        var item = ManualVendorSegment.NewManualVendorSegment("code-775", "title-238");
        Check.Equal(item.Status, ManualVendorSegmentStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualVendorSegmentMovesToApproved()
    {
        var item = ManualVendorSegment.NewManualVendorSegment("code-775", "title-238");
        try
        {
            item.MoveTo(ManualVendorSegmentStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualVendorSegmentStatus.Approved, "status");
    }

    public static void ManualVendorSegmentRejectsMovesFromWithdrawn()
    {
        var item = ManualVendorSegment.NewManualVendorSegment("code-775", "title-238");
        item.Status = ManualVendorSegmentStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ManualVendorSegmentStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ManualVendorSegmentStartsInProposed();
        ManualVendorSegmentMovesToApproved();
        ManualVendorSegmentRejectsMovesFromWithdrawn();
    }
}
