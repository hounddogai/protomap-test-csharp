namespace Acme.Workforce.Assets.Tests.Laptop.Forecast;

using Acme.Workforce.Assets.Laptop.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class LaptopForecastChecks
{
    public static void LaptopForecastStartsInIdle()
    {
        var item = LaptopForecast.NewLaptopForecast("code-812", "title-523");
        Check.Equal(item.Status, LaptopForecastStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopForecastMovesToStopped()
    {
        var item = LaptopForecast.NewLaptopForecast("code-812", "title-523");
        try
        {
            item.MoveTo(LaptopForecastStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopForecastStatus.Stopped, "status");
    }

    public static void LaptopForecastRejectsMovesFromStopped()
    {
        var item = LaptopForecast.NewLaptopForecast("code-812", "title-523");
        item.Status = LaptopForecastStatus.Stopped;
        Check.Fails(() => item.MoveTo(LaptopForecastStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LaptopForecastStartsInIdle();
        LaptopForecastMovesToStopped();
        LaptopForecastRejectsMovesFromStopped();
    }
}
