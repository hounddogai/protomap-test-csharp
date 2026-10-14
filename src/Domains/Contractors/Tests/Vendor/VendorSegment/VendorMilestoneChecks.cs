namespace Acme.Workforce.Contractors.Tests.Vendor.VendorSegment;

using Acme.Workforce.Contractors.Vendor.VendorSegment;

/// <summary>Checks of vendor segment code, run by the project's runner.</summary>
public static class VendorMilestoneChecks
{
    public static void VendorMilestoneStartsInPlanned()
    {
        var item = VendorMilestone.NewVendorMilestone("code-73", "title-505");
        Check.Equal(item.Status, VendorMilestoneStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VendorMilestoneMovesToCompleted()
    {
        var item = VendorMilestone.NewVendorMilestone("code-73", "title-505");
        try
        {
            item.MoveTo(VendorMilestoneStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VendorMilestoneStatus.Completed, "status");
    }

    public static void VendorMilestoneRejectsMovesFromCancelled()
    {
        var item = VendorMilestone.NewVendorMilestone("code-73", "title-505");
        item.Status = VendorMilestoneStatus.Cancelled;
        Check.Fails(() => item.MoveTo(VendorMilestoneStatus.Planned), "move");
    }

    public static void RunAll()
    {
        VendorMilestoneStartsInPlanned();
        VendorMilestoneMovesToCompleted();
        VendorMilestoneRejectsMovesFromCancelled();
    }
}
