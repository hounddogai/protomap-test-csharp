namespace Acme.Workforce.Assets.Tests.LicenseSeat.Forecast;

using Acme.Workforce.Assets.LicenseSeat.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class LicenseSeatForecastChecks
{
    public static void LicenseSeatForecastStartsInIdle()
    {
        var item = LicenseSeatForecast.NewLicenseSeatForecast("code-769", "name-517");
        Check.Equal(item.Status, LicenseSeatForecastStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LicenseSeatForecastMovesToStopped()
    {
        var item = LicenseSeatForecast.NewLicenseSeatForecast("code-769", "name-517");
        try
        {
            item.MoveTo(LicenseSeatForecastStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LicenseSeatForecastStatus.Stopped, "status");
    }

    public static void LicenseSeatForecastRejectsMovesFromStopped()
    {
        var item = LicenseSeatForecast.NewLicenseSeatForecast("code-769", "name-517");
        item.Status = LicenseSeatForecastStatus.Stopped;
        Check.Fails(() => item.MoveTo(LicenseSeatForecastStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LicenseSeatForecastStartsInIdle();
        LicenseSeatForecastMovesToStopped();
        LicenseSeatForecastRejectsMovesFromStopped();
    }
}
