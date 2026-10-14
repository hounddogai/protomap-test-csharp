namespace Acme.Workforce.Assets.Tests.LicenseSeat.Reservation;

using Acme.Workforce.Assets.LicenseSeat.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class LicenseSeatScheduleChecks
{
    public static void LicenseSeatScheduleStartsInProposed()
    {
        var item = LicenseSeatSchedule.NewLicenseSeatSchedule("code-723", "name-412");
        Check.Equal(item.Status, LicenseSeatScheduleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LicenseSeatScheduleMovesToApproved()
    {
        var item = LicenseSeatSchedule.NewLicenseSeatSchedule("code-723", "name-412");
        try
        {
            item.MoveTo(LicenseSeatScheduleStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LicenseSeatScheduleStatus.Approved, "status");
    }

    public static void LicenseSeatScheduleRejectsMovesFromWithdrawn()
    {
        var item = LicenseSeatSchedule.NewLicenseSeatSchedule("code-723", "name-412");
        item.Status = LicenseSeatScheduleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LicenseSeatScheduleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LicenseSeatScheduleStartsInProposed();
        LicenseSeatScheduleMovesToApproved();
        LicenseSeatScheduleRejectsMovesFromWithdrawn();
    }
}
