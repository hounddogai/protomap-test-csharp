namespace Acme.Workforce.Assets.Tests.LicenseSeat.Agreement;

using Acme.Workforce.Assets.LicenseSeat.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class LenientLicenseSeatScheduleChecks
{
    public static void LenientLicenseSeatScheduleStartsInRequested()
    {
        var item = LenientLicenseSeatSchedule.NewLenientLicenseSeatSchedule("code-531", "label-125");
        Check.Equal(item.Status, LenientLicenseSeatScheduleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientLicenseSeatScheduleMovesToGranted()
    {
        var item = LenientLicenseSeatSchedule.NewLenientLicenseSeatSchedule("code-531", "label-125");
        try
        {
            item.MoveTo(LenientLicenseSeatScheduleStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientLicenseSeatScheduleStatus.Granted, "status");
    }

    public static void LenientLicenseSeatScheduleRejectsMovesFromExpired()
    {
        var item = LenientLicenseSeatSchedule.NewLenientLicenseSeatSchedule("code-531", "label-125");
        item.Status = LenientLicenseSeatScheduleStatus.Expired;
        Check.Fails(() => item.MoveTo(LenientLicenseSeatScheduleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LenientLicenseSeatScheduleStartsInRequested();
        LenientLicenseSeatScheduleMovesToGranted();
        LenientLicenseSeatScheduleRejectsMovesFromExpired();
    }
}
