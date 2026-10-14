namespace Acme.Workforce.Contractors.Tests.Vendor.VendorBatch;

using Acme.Workforce.Contractors.Vendor.VendorBatch;

/// <summary>Checks of vendor batch code, run by the project's runner.</summary>
public static class LegacyVendorAssignmentChecks
{
    public static void LegacyVendorAssignmentStartsInIdle()
    {
        var item = LegacyVendorAssignment.NewLegacyVendorAssignment("code-975", "label-870");
        Check.Equal(item.Status, LegacyVendorAssignmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyVendorAssignmentMovesToReady()
    {
        var item = LegacyVendorAssignment.NewLegacyVendorAssignment("code-975", "label-870");
        try
        {
            item.MoveTo(LegacyVendorAssignmentStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyVendorAssignmentStatus.Ready, "status");
    }

    public static void LegacyVendorAssignmentRejectsMovesFromStopped()
    {
        var item = LegacyVendorAssignment.NewLegacyVendorAssignment("code-975", "label-870");
        item.Status = LegacyVendorAssignmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(LegacyVendorAssignmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LegacyVendorAssignmentStartsInIdle();
        LegacyVendorAssignmentMovesToReady();
        LegacyVendorAssignmentRejectsMovesFromStopped();
    }
}
