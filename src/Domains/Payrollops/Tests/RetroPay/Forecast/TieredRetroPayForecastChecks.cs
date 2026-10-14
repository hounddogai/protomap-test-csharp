namespace Acme.Workforce.Payrollops.Tests.RetroPay.Forecast;

using Acme.Workforce.Payrollops.RetroPay.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class TieredRetroPayForecastChecks
{
    public static void TieredRetroPayForecastStartsInCreated()
    {
        var item = TieredRetroPayForecast.NewTieredRetroPayForecast("code-271", "name-817");
        Check.Equal(item.Status, TieredRetroPayForecastStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredRetroPayForecastMovesToValidated()
    {
        var item = TieredRetroPayForecast.NewTieredRetroPayForecast("code-271", "name-817");
        try
        {
            item.MoveTo(TieredRetroPayForecastStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredRetroPayForecastStatus.Validated, "status");
    }

    public static void TieredRetroPayForecastRejectsMovesFromRetired()
    {
        var item = TieredRetroPayForecast.NewTieredRetroPayForecast("code-271", "name-817");
        item.Status = TieredRetroPayForecastStatus.Retired;
        Check.Fails(() => item.MoveTo(TieredRetroPayForecastStatus.Created), "move");
    }

    public static void RunAll()
    {
        TieredRetroPayForecastStartsInCreated();
        TieredRetroPayForecastMovesToValidated();
        TieredRetroPayForecastRejectsMovesFromRetired();
    }
}
