namespace Acme.Workforce.Contractors.Tests.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

/// <summary>Checks of default vendor schedule code, run by the project's runner.</summary>
public static class ManualVendorMilestoneChecks
{
    public static void ManualVendorMilestoneStartsInIdle()
    {
        var item = ManualVendorMilestone.NewManualVendorMilestone("code-137", "name-799");
        Check.Equal(item.Status, ManualVendorMilestoneStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualVendorMilestoneMovesToReady()
    {
        var item = ManualVendorMilestone.NewManualVendorMilestone("code-137", "name-799");
        try
        {
            item.MoveTo(ManualVendorMilestoneStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualVendorMilestoneStatus.Ready, "status");
    }

    public static void ManualVendorMilestoneRejectsMovesFromStopped()
    {
        var item = ManualVendorMilestone.NewManualVendorMilestone("code-137", "name-799");
        item.Status = ManualVendorMilestoneStatus.Stopped;
        Check.Fails(() => item.MoveTo(ManualVendorMilestoneStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ManualVendorMilestoneStartsInIdle();
        ManualVendorMilestoneMovesToReady();
        ManualVendorMilestoneRejectsMovesFromStopped();
    }
}
