namespace Acme.Workforce.Workplace.Tests.Occupancy.Draft;

using Acme.Workforce.Workplace.Occupancy.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class OccupancyForecastChecks
{
    public static void OccupancyForecastStartsInProposed()
    {
        var item = OccupancyForecast.NewOccupancyForecast("code-24", "title-518");
        Check.Equal(item.Status, OccupancyForecastStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancyForecastMovesToRejected()
    {
        var item = OccupancyForecast.NewOccupancyForecast("code-24", "title-518");
        try
        {
            item.MoveTo(OccupancyForecastStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancyForecastStatus.Rejected, "status");
    }

    public static void OccupancyForecastRejectsMovesFromWithdrawn()
    {
        var item = OccupancyForecast.NewOccupancyForecast("code-24", "title-518");
        item.Status = OccupancyForecastStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(OccupancyForecastStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        OccupancyForecastStartsInProposed();
        OccupancyForecastMovesToRejected();
        OccupancyForecastRejectsMovesFromWithdrawn();
    }
}
