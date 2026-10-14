namespace Acme.Workforce.Assets.Tests.Refresh.Forecast;

using Acme.Workforce.Assets.Refresh.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class RefreshForecastChecks
{
    public static void RefreshForecastStartsInRequested()
    {
        var item = RefreshForecast.NewRefreshForecast("code-393", "title-741");
        Check.Equal(item.Status, RefreshForecastStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RefreshForecastMovesToGranted()
    {
        var item = RefreshForecast.NewRefreshForecast("code-393", "title-741");
        try
        {
            item.MoveTo(RefreshForecastStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RefreshForecastStatus.Granted, "status");
    }

    public static void RefreshForecastRejectsMovesFromExpired()
    {
        var item = RefreshForecast.NewRefreshForecast("code-393", "title-741");
        item.Status = RefreshForecastStatus.Expired;
        Check.Fails(() => item.MoveTo(RefreshForecastStatus.Requested), "move");
    }

    public static void RunAll()
    {
        RefreshForecastStartsInRequested();
        RefreshForecastMovesToGranted();
        RefreshForecastRejectsMovesFromExpired();
    }
}
