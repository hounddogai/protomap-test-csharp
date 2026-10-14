namespace Acme.Workforce.Compensation.Tests.EquityGrant.Forecast;

using Acme.Workforce.Compensation.EquityGrant.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class EquityGrantForecastChecks
{
    public static void EquityGrantForecastStartsInDraft()
    {
        var item = EquityGrantForecast.NewEquityGrantForecast("code-966", "title-442");
        Check.Equal(item.Status, EquityGrantForecastStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EquityGrantForecastMovesToActive()
    {
        var item = EquityGrantForecast.NewEquityGrantForecast("code-966", "title-442");
        try
        {
            item.MoveTo(EquityGrantForecastStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EquityGrantForecastStatus.Active, "status");
    }

    public static void EquityGrantForecastRejectsMovesFromClosed()
    {
        var item = EquityGrantForecast.NewEquityGrantForecast("code-966", "title-442");
        item.Status = EquityGrantForecastStatus.Closed;
        Check.Fails(() => item.MoveTo(EquityGrantForecastStatus.Draft), "move");
    }

    public static void RunAll()
    {
        EquityGrantForecastStartsInDraft();
        EquityGrantForecastMovesToActive();
        EquityGrantForecastRejectsMovesFromClosed();
    }
}
