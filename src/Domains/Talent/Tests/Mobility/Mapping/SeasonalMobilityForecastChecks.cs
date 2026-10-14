namespace Acme.Workforce.Talent.Tests.Mobility.Mapping;

using Acme.Workforce.Talent.Mobility.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class SeasonalMobilityForecastChecks
{
    public static void SeasonalMobilityForecastStartsInDraft()
    {
        var item = SeasonalMobilityForecast.NewSeasonalMobilityForecast("code-639", "label-248");
        Check.Equal(item.Status, SeasonalMobilityForecastStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalMobilityForecastMovesToActive()
    {
        var item = SeasonalMobilityForecast.NewSeasonalMobilityForecast("code-639", "label-248");
        try
        {
            item.MoveTo(SeasonalMobilityForecastStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalMobilityForecastStatus.Active, "status");
    }

    public static void SeasonalMobilityForecastRejectsMovesFromClosed()
    {
        var item = SeasonalMobilityForecast.NewSeasonalMobilityForecast("code-639", "label-248");
        item.Status = SeasonalMobilityForecastStatus.Closed;
        Check.Fails(() => item.MoveTo(SeasonalMobilityForecastStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SeasonalMobilityForecastStartsInDraft();
        SeasonalMobilityForecastMovesToActive();
        SeasonalMobilityForecastRejectsMovesFromClosed();
    }
}
