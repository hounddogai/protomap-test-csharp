namespace Acme.Workforce.Assets.Tests.LicenseSeat.StrictSeatReservation;

using Acme.Workforce.Assets.LicenseSeat.StrictSeatReservation;

/// <summary>Checks of strict seat reservation code, run by the project's runner.</summary>
public static class LicenseSeatGroupChecks
{
    public static void LicenseSeatGroupStartsInRequested()
    {
        var item = LicenseSeatGroup.NewLicenseSeatGroup("code-878", "label-817");
        Check.Equal(item.Status, LicenseSeatGroupStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LicenseSeatGroupMovesToGranted()
    {
        var item = LicenseSeatGroup.NewLicenseSeatGroup("code-878", "label-817");
        try
        {
            item.MoveTo(LicenseSeatGroupStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LicenseSeatGroupStatus.Granted, "status");
    }

    public static void LicenseSeatGroupRejectsMovesFromExpired()
    {
        var item = LicenseSeatGroup.NewLicenseSeatGroup("code-878", "label-817");
        item.Status = LicenseSeatGroupStatus.Expired;
        Check.Fails(() => item.MoveTo(LicenseSeatGroupStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LicenseSeatGroupStartsInRequested();
        LicenseSeatGroupMovesToGranted();
        LicenseSeatGroupRejectsMovesFromExpired();
    }
}
