namespace Acme.Workforce.Contractors.Tests.Vendor.Schedule;

using Acme.Workforce.Contractors.Vendor.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ScheduledVendorScheduleChecks
{
    public static void ScheduledVendorScheduleStartsInRequested()
    {
        var item = ScheduledVendorSchedule.NewScheduledVendorSchedule("code-536", "title-856");
        Check.Equal(item.Status, ScheduledVendorScheduleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledVendorScheduleMovesToRevoked()
    {
        var item = ScheduledVendorSchedule.NewScheduledVendorSchedule("code-536", "title-856");
        try
        {
            item.MoveTo(ScheduledVendorScheduleStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledVendorScheduleStatus.Revoked, "status");
    }

    public static void ScheduledVendorScheduleRejectsMovesFromExpired()
    {
        var item = ScheduledVendorSchedule.NewScheduledVendorSchedule("code-536", "title-856");
        item.Status = ScheduledVendorScheduleStatus.Expired;
        Check.Fails(() => item.MoveTo(ScheduledVendorScheduleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ScheduledVendorScheduleStartsInRequested();
        ScheduledVendorScheduleMovesToRevoked();
        ScheduledVendorScheduleRejectsMovesFromExpired();
    }
}
