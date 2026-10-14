namespace Acme.Workforce.Compensation.Tests.PayBand.Forecast;

using Acme.Workforce.Compensation.PayBand.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class ScheduledPayBandForecastChecks
{
    public static void ScheduledPayBandForecastStartsInPlanned()
    {
        var item = ScheduledPayBandForecast.NewScheduledPayBandForecast("code-669", "label-428");
        Check.Equal(item.Status, ScheduledPayBandForecastStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledPayBandForecastMovesToScheduled()
    {
        var item = ScheduledPayBandForecast.NewScheduledPayBandForecast("code-669", "label-428");
        try
        {
            item.MoveTo(ScheduledPayBandForecastStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledPayBandForecastStatus.Scheduled, "status");
    }

    public static void ScheduledPayBandForecastRejectsMovesFromCancelled()
    {
        var item = ScheduledPayBandForecast.NewScheduledPayBandForecast("code-669", "label-428");
        item.Status = ScheduledPayBandForecastStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ScheduledPayBandForecastStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ScheduledPayBandForecastStartsInPlanned();
        ScheduledPayBandForecastMovesToScheduled();
        ScheduledPayBandForecastRejectsMovesFromCancelled();
    }
}
