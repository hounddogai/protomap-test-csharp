namespace Acme.Workforce.Assets.Tests.LicenseSeat.Baseline;

using Acme.Workforce.Assets.LicenseSeat.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PrimaryLicenseSeatWindowChecks
{
    public static void PrimaryLicenseSeatWindowStartsInIdle()
    {
        var item = PrimaryLicenseSeatWindow.NewPrimaryLicenseSeatWindow("code-812", "label-136");
        Check.Equal(item.Status, PrimaryLicenseSeatWindowStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryLicenseSeatWindowMovesToReady()
    {
        var item = PrimaryLicenseSeatWindow.NewPrimaryLicenseSeatWindow("code-812", "label-136");
        try
        {
            item.MoveTo(PrimaryLicenseSeatWindowStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryLicenseSeatWindowStatus.Ready, "status");
    }

    public static void PrimaryLicenseSeatWindowRejectsMovesFromStopped()
    {
        var item = PrimaryLicenseSeatWindow.NewPrimaryLicenseSeatWindow("code-812", "label-136");
        item.Status = PrimaryLicenseSeatWindowStatus.Stopped;
        Check.Fails(() => item.MoveTo(PrimaryLicenseSeatWindowStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PrimaryLicenseSeatWindowStartsInIdle();
        PrimaryLicenseSeatWindowMovesToReady();
        PrimaryLicenseSeatWindowRejectsMovesFromStopped();
    }
}
