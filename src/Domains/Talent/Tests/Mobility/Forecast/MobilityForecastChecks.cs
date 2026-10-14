namespace Acme.Workforce.Talent.Tests.Mobility.Forecast;

using Acme.Workforce.Talent.Mobility.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class MobilityForecastChecks
{
    public static void MobilityForecastStartsInNew()
    {
        var item = MobilityForecast.NewMobilityForecast("code-269", "name-404");
        Check.Equal(item.Status, MobilityForecastStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MobilityForecastMovesToFailed()
    {
        var item = MobilityForecast.NewMobilityForecast("code-269", "name-404");
        try
        {
            item.MoveTo(MobilityForecastStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MobilityForecastStatus.Failed, "status");
    }

    public static void MobilityForecastRejectsMovesFromFailed()
    {
        var item = MobilityForecast.NewMobilityForecast("code-269", "name-404");
        item.Status = MobilityForecastStatus.Failed;
        Check.Fails(() => item.MoveTo(MobilityForecastStatus.New), "move");
    }

    public static void RunAll()
    {
        MobilityForecastStartsInNew();
        MobilityForecastMovesToFailed();
        MobilityForecastRejectsMovesFromFailed();
    }
}
