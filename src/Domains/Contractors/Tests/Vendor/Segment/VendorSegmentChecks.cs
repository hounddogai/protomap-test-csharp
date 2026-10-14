namespace Acme.Workforce.Contractors.Tests.Vendor.Segment;

using Acme.Workforce.Contractors.Vendor.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class VendorSegmentChecks
{
    public static void VendorSegmentStartsInDraft()
    {
        var item = VendorSegment.NewVendorSegment("code-708", "title-369");
        Check.Equal(item.Status, VendorSegmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VendorSegmentMovesToSuspended()
    {
        var item = VendorSegment.NewVendorSegment("code-708", "title-369");
        try
        {
            item.MoveTo(VendorSegmentStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VendorSegmentStatus.Suspended, "status");
    }

    public static void VendorSegmentRejectsMovesFromClosed()
    {
        var item = VendorSegment.NewVendorSegment("code-708", "title-369");
        item.Status = VendorSegmentStatus.Closed;
        Check.Fails(() => item.MoveTo(VendorSegmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        VendorSegmentStartsInDraft();
        VendorSegmentMovesToSuspended();
        VendorSegmentRejectsMovesFromClosed();
    }
}
