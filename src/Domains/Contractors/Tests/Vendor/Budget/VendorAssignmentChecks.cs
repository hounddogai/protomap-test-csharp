namespace Acme.Workforce.Contractors.Tests.Vendor.Budget;

using Acme.Workforce.Contractors.Vendor.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class VendorAssignmentChecks
{
    public static void VendorAssignmentStartsInProposed()
    {
        var item = VendorAssignment.NewVendorAssignment("code-126", "title-886");
        Check.Equal(item.Status, VendorAssignmentStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VendorAssignmentMovesToWithdrawn()
    {
        var item = VendorAssignment.NewVendorAssignment("code-126", "title-886");
        try
        {
            item.MoveTo(VendorAssignmentStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VendorAssignmentStatus.Withdrawn, "status");
    }

    public static void VendorAssignmentRejectsMovesFromWithdrawn()
    {
        var item = VendorAssignment.NewVendorAssignment("code-126", "title-886");
        item.Status = VendorAssignmentStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(VendorAssignmentStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        VendorAssignmentStartsInProposed();
        VendorAssignmentMovesToWithdrawn();
        VendorAssignmentRejectsMovesFromWithdrawn();
    }
}
