namespace Acme.Workforce.Contractors.Tests.Vendor.Score;

using Acme.Workforce.Contractors.Vendor.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class AdaptiveVendorAssignmentChecks
{
    public static void AdaptiveVendorAssignmentStartsInProposed()
    {
        var item = AdaptiveVendorAssignment.NewAdaptiveVendorAssignment("code-657", "title-8");
        Check.Equal(item.Status, AdaptiveVendorAssignmentStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveVendorAssignmentMovesToRejected()
    {
        var item = AdaptiveVendorAssignment.NewAdaptiveVendorAssignment("code-657", "title-8");
        try
        {
            item.MoveTo(AdaptiveVendorAssignmentStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveVendorAssignmentStatus.Rejected, "status");
    }

    public static void AdaptiveVendorAssignmentRejectsMovesFromWithdrawn()
    {
        var item = AdaptiveVendorAssignment.NewAdaptiveVendorAssignment("code-657", "title-8");
        item.Status = AdaptiveVendorAssignmentStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AdaptiveVendorAssignmentStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AdaptiveVendorAssignmentStartsInProposed();
        AdaptiveVendorAssignmentMovesToRejected();
        AdaptiveVendorAssignmentRejectsMovesFromWithdrawn();
    }
}
