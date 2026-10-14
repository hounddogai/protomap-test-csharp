namespace Acme.Workforce.Payrollops.Tests.Garnishment.Forecast;

using Acme.Workforce.Payrollops.Garnishment.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class GarnishmentForecastChecks
{
    public static void GarnishmentForecastStartsInNew()
    {
        var item = GarnishmentForecast.NewGarnishmentForecast("code-406", "name-44");
        Check.Equal(item.Status, GarnishmentForecastStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GarnishmentForecastMovesToQueued()
    {
        var item = GarnishmentForecast.NewGarnishmentForecast("code-406", "name-44");
        try
        {
            item.MoveTo(GarnishmentForecastStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GarnishmentForecastStatus.Queued, "status");
    }

    public static void GarnishmentForecastRejectsMovesFromFailed()
    {
        var item = GarnishmentForecast.NewGarnishmentForecast("code-406", "name-44");
        item.Status = GarnishmentForecastStatus.Failed;
        Check.Fails(() => item.MoveTo(GarnishmentForecastStatus.New), "move");
    }

    public static void RunAll()
    {
        GarnishmentForecastStartsInNew();
        GarnishmentForecastMovesToQueued();
        GarnishmentForecastRejectsMovesFromFailed();
    }
}
