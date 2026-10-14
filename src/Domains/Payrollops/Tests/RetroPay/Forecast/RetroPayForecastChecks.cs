namespace Acme.Workforce.Payrollops.Tests.RetroPay.Forecast;

using Acme.Workforce.Payrollops.RetroPay.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class RetroPayForecastChecks
{
    public static void RetroPayForecastStartsInCreated()
    {
        var item = RetroPayForecast.NewRetroPayForecast("code-777", "title-1");
        Check.Equal(item.Status, RetroPayForecastStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPayForecastMovesToPublished()
    {
        var item = RetroPayForecast.NewRetroPayForecast("code-777", "title-1");
        try
        {
            item.MoveTo(RetroPayForecastStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPayForecastStatus.Published, "status");
    }

    public static void RetroPayForecastRejectsMovesFromRetired()
    {
        var item = RetroPayForecast.NewRetroPayForecast("code-777", "title-1");
        item.Status = RetroPayForecastStatus.Retired;
        Check.Fails(() => item.MoveTo(RetroPayForecastStatus.Created), "move");
    }

    public static void RunAll()
    {
        RetroPayForecastStartsInCreated();
        RetroPayForecastMovesToPublished();
        RetroPayForecastRejectsMovesFromRetired();
    }
}
