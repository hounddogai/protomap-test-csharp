namespace Acme.Workforce.Assets.Tests.LicenseSeat.Bundle;

using Acme.Workforce.Assets.LicenseSeat.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class SeasonalLicenseSeatWindowChecks
{
    public static void SeasonalLicenseSeatWindowStartsInIdle()
    {
        var item = SeasonalLicenseSeatWindow.NewSeasonalLicenseSeatWindow("code-626", "title-437");
        Check.Equal(item.Status, SeasonalLicenseSeatWindowStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalLicenseSeatWindowMovesToWarming()
    {
        var item = SeasonalLicenseSeatWindow.NewSeasonalLicenseSeatWindow("code-626", "title-437");
        try
        {
            item.MoveTo(SeasonalLicenseSeatWindowStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalLicenseSeatWindowStatus.Warming, "status");
    }

    public static void SeasonalLicenseSeatWindowRejectsMovesFromStopped()
    {
        var item = SeasonalLicenseSeatWindow.NewSeasonalLicenseSeatWindow("code-626", "title-437");
        item.Status = SeasonalLicenseSeatWindowStatus.Stopped;
        Check.Fails(() => item.MoveTo(SeasonalLicenseSeatWindowStatus.Idle), "move");
    }

    public static void RunAll()
    {
        SeasonalLicenseSeatWindowStartsInIdle();
        SeasonalLicenseSeatWindowMovesToWarming();
        SeasonalLicenseSeatWindowRejectsMovesFromStopped();
    }
}
