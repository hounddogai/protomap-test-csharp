namespace Acme.Workforce.Assets.Tests.Warranty.Forecast;

using Acme.Workforce.Assets.Warranty.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class WarrantyForecastChecks
{
    public static void WarrantyForecastStartsInIdle()
    {
        var item = WarrantyForecast.NewWarrantyForecast("code-258", "title-193");
        Check.Equal(item.Status, WarrantyForecastStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantyForecastMovesToDraining()
    {
        var item = WarrantyForecast.NewWarrantyForecast("code-258", "title-193");
        try
        {
            item.MoveTo(WarrantyForecastStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantyForecastStatus.Draining, "status");
    }

    public static void WarrantyForecastRejectsMovesFromStopped()
    {
        var item = WarrantyForecast.NewWarrantyForecast("code-258", "title-193");
        item.Status = WarrantyForecastStatus.Stopped;
        Check.Fails(() => item.MoveTo(WarrantyForecastStatus.Idle), "move");
    }

    public static void RunAll()
    {
        WarrantyForecastStartsInIdle();
        WarrantyForecastMovesToDraining();
        WarrantyForecastRejectsMovesFromStopped();
    }
}
