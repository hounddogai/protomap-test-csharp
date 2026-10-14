namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatMilestone;

using Acme.Workforce.Assets.LicenseSeat.SeatMilestone;

/// <summary>Checks of seat milestone code, run by the project's runner.</summary>
public static class LicenseSeatOverrideChecks
{
    public static void LicenseSeatOverrideStartsInIdle()
    {
        var item = LicenseSeatOverride.NewLicenseSeatOverride("code-417", "label-382");
        Check.Equal(item.Status, LicenseSeatOverrideStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LicenseSeatOverrideMovesToStopped()
    {
        var item = LicenseSeatOverride.NewLicenseSeatOverride("code-417", "label-382");
        try
        {
            item.MoveTo(LicenseSeatOverrideStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LicenseSeatOverrideStatus.Stopped, "status");
    }

    public static void LicenseSeatOverrideRejectsMovesFromStopped()
    {
        var item = LicenseSeatOverride.NewLicenseSeatOverride("code-417", "label-382");
        item.Status = LicenseSeatOverrideStatus.Stopped;
        Check.Fails(() => item.MoveTo(LicenseSeatOverrideStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LicenseSeatOverrideStartsInIdle();
        LicenseSeatOverrideMovesToStopped();
        LicenseSeatOverrideRejectsMovesFromStopped();
    }
}
