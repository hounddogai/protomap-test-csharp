namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatMilestone;

using Acme.Workforce.Assets.LicenseSeat.SeatMilestone;

/// <summary>Checks of seat milestone code, run by the project's runner.</summary>
public static class LenientLicenseSeatMilestoneChecks
{
    public static void LenientLicenseSeatMilestoneStartsInRequested()
    {
        var item = LenientLicenseSeatMilestone.NewLenientLicenseSeatMilestone("code-743", "name-963");
        Check.Equal(item.Status, LenientLicenseSeatMilestoneStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientLicenseSeatMilestoneMovesToGranted()
    {
        var item = LenientLicenseSeatMilestone.NewLenientLicenseSeatMilestone("code-743", "name-963");
        try
        {
            item.MoveTo(LenientLicenseSeatMilestoneStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientLicenseSeatMilestoneStatus.Granted, "status");
    }

    public static void LenientLicenseSeatMilestoneRejectsMovesFromExpired()
    {
        var item = LenientLicenseSeatMilestone.NewLenientLicenseSeatMilestone("code-743", "name-963");
        item.Status = LenientLicenseSeatMilestoneStatus.Expired;
        Check.Fails(() => item.MoveTo(LenientLicenseSeatMilestoneStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LenientLicenseSeatMilestoneStartsInRequested();
        LenientLicenseSeatMilestoneMovesToGranted();
        LenientLicenseSeatMilestoneRejectsMovesFromExpired();
    }
}
