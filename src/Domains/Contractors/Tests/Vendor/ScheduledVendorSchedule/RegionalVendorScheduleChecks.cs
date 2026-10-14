namespace Acme.Workforce.Contractors.Tests.Vendor.ScheduledVendorSchedule;

using Acme.Workforce.Contractors.Vendor.ScheduledVendorSchedule;

/// <summary>Checks of scheduled vendor schedule code, run by the project's runner.</summary>
public static class RegionalVendorScheduleChecks
{
    public static void RegionalVendorScheduleStartsInIdle()
    {
        var item = RegionalVendorSchedule.NewRegionalVendorSchedule("code-919", "title-847");
        Check.Equal(item.Status, RegionalVendorScheduleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalVendorScheduleMovesToReady()
    {
        var item = RegionalVendorSchedule.NewRegionalVendorSchedule("code-919", "title-847");
        try
        {
            item.MoveTo(RegionalVendorScheduleStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalVendorScheduleStatus.Ready, "status");
    }

    public static void RegionalVendorScheduleRejectsMovesFromStopped()
    {
        var item = RegionalVendorSchedule.NewRegionalVendorSchedule("code-919", "title-847");
        item.Status = RegionalVendorScheduleStatus.Stopped;
        Check.Fails(() => item.MoveTo(RegionalVendorScheduleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RegionalVendorScheduleStartsInIdle();
        RegionalVendorScheduleMovesToReady();
        RegionalVendorScheduleRejectsMovesFromStopped();
    }
}
