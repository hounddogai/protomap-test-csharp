namespace Acme.Workforce.Contractors.Tests.Vendor.VendorSegment;

using Acme.Workforce.Contractors.Vendor.VendorSegment;

/// <summary>Checks of vendor segment code, run by the project's runner.</summary>
public static class VendorThresholdChecks
{
    public static void VendorThresholdStartsInPlanned()
    {
        var item = VendorThreshold.NewVendorThreshold("code-603", "name-36");
        Check.Equal(item.Status, VendorThresholdStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VendorThresholdMovesToCompleted()
    {
        var item = VendorThreshold.NewVendorThreshold("code-603", "name-36");
        try
        {
            item.MoveTo(VendorThresholdStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VendorThresholdStatus.Completed, "status");
    }

    public static void VendorThresholdRejectsMovesFromCancelled()
    {
        var item = VendorThreshold.NewVendorThreshold("code-603", "name-36");
        item.Status = VendorThresholdStatus.Cancelled;
        Check.Fails(() => item.MoveTo(VendorThresholdStatus.Planned), "move");
    }

    public static void RunAll()
    {
        VendorThresholdStartsInPlanned();
        VendorThresholdMovesToCompleted();
        VendorThresholdRejectsMovesFromCancelled();
    }
}
