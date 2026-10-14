namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatMilestone;

using Acme.Workforce.Assets.LicenseSeat.SeatMilestone;

/// <summary>Checks of seat milestone code, run by the project's runner.</summary>
public static class LicenseSeatAssignmentChecks
{
    public static void LicenseSeatAssignmentStartsInIdle()
    {
        var item = LicenseSeatAssignment.NewLicenseSeatAssignment("code-889", "label-143");
        Check.Equal(item.Status, LicenseSeatAssignmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LicenseSeatAssignmentMovesToDraining()
    {
        var item = LicenseSeatAssignment.NewLicenseSeatAssignment("code-889", "label-143");
        try
        {
            item.MoveTo(LicenseSeatAssignmentStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LicenseSeatAssignmentStatus.Draining, "status");
    }

    public static void LicenseSeatAssignmentRejectsMovesFromStopped()
    {
        var item = LicenseSeatAssignment.NewLicenseSeatAssignment("code-889", "label-143");
        item.Status = LicenseSeatAssignmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(LicenseSeatAssignmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LicenseSeatAssignmentStartsInIdle();
        LicenseSeatAssignmentMovesToDraining();
        LicenseSeatAssignmentRejectsMovesFromStopped();
    }
}
