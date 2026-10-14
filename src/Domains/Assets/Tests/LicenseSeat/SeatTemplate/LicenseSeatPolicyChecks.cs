namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatTemplate;

using Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

/// <summary>Checks of seat template code, run by the project's runner.</summary>
public static class LicenseSeatPolicyChecks
{
    public static void LicenseSeatPolicyStartsInRequested()
    {
        var item = LicenseSeatPolicy.NewLicenseSeatPolicy("code-492", "title-208");
        Check.Equal(item.Status, LicenseSeatPolicyStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LicenseSeatPolicyMovesToGranted()
    {
        var item = LicenseSeatPolicy.NewLicenseSeatPolicy("code-492", "title-208");
        try
        {
            item.MoveTo(LicenseSeatPolicyStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LicenseSeatPolicyStatus.Granted, "status");
    }

    public static void LicenseSeatPolicyRejectsMovesFromExpired()
    {
        var item = LicenseSeatPolicy.NewLicenseSeatPolicy("code-492", "title-208");
        item.Status = LicenseSeatPolicyStatus.Expired;
        Check.Fails(() => item.MoveTo(LicenseSeatPolicyStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LicenseSeatPolicyStartsInRequested();
        LicenseSeatPolicyMovesToGranted();
        LicenseSeatPolicyRejectsMovesFromExpired();
    }
}
