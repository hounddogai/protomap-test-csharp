namespace Acme.Workforce.Workplace.Tests.Occupancy.Checkpoint;

using Acme.Workforce.Workplace.Occupancy.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class WeightedOccupancyForecastChecks
{
    public static void WeightedOccupancyForecastStartsInOpen()
    {
        var item = WeightedOccupancyForecast.NewWeightedOccupancyForecast("code-661", "name-625");
        Check.Equal(item.Status, WeightedOccupancyForecastStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedOccupancyForecastMovesToInReview()
    {
        var item = WeightedOccupancyForecast.NewWeightedOccupancyForecast("code-661", "name-625");
        try
        {
            item.MoveTo(WeightedOccupancyForecastStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedOccupancyForecastStatus.InReview, "status");
    }

    public static void WeightedOccupancyForecastRejectsMovesFromArchived()
    {
        var item = WeightedOccupancyForecast.NewWeightedOccupancyForecast("code-661", "name-625");
        item.Status = WeightedOccupancyForecastStatus.Archived;
        Check.Fails(() => item.MoveTo(WeightedOccupancyForecastStatus.Open), "move");
    }

    public static void RunAll()
    {
        WeightedOccupancyForecastStartsInOpen();
        WeightedOccupancyForecastMovesToInReview();
        WeightedOccupancyForecastRejectsMovesFromArchived();
    }
}
