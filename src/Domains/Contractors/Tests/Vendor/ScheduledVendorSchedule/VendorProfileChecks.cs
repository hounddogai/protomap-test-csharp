namespace Acme.Workforce.Contractors.Tests.Vendor.ScheduledVendorSchedule;

using Acme.Workforce.Contractors.Vendor.ScheduledVendorSchedule;

/// <summary>Checks of scheduled vendor schedule code, run by the project's runner.</summary>
public static class VendorProfileChecks
{
    public static void VendorProfileStartsInProposed()
    {
        var item = VendorProfile.NewVendorProfile("code-66", "label-233");
        Check.Equal(item.Status, VendorProfileStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VendorProfileMovesToApproved()
    {
        var item = VendorProfile.NewVendorProfile("code-66", "label-233");
        try
        {
            item.MoveTo(VendorProfileStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VendorProfileStatus.Approved, "status");
    }

    public static void VendorProfileRejectsMovesFromWithdrawn()
    {
        var item = VendorProfile.NewVendorProfile("code-66", "label-233");
        item.Status = VendorProfileStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(VendorProfileStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        VendorProfileStartsInProposed();
        VendorProfileMovesToApproved();
        VendorProfileRejectsMovesFromWithdrawn();
    }
}
