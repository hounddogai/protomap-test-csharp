namespace Acme.Workforce.Workplace.Tests.Occupancy.Draft;

using Acme.Workforce.Workplace.Occupancy.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class GlobalOccupancyForecastChecks
{
    public static void GlobalOccupancyForecastStartsInRequested()
    {
        var item = GlobalOccupancyForecast.NewGlobalOccupancyForecast("code-378", "label-28");
        Check.Equal(item.Status, GlobalOccupancyForecastStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalOccupancyForecastMovesToRevoked()
    {
        var item = GlobalOccupancyForecast.NewGlobalOccupancyForecast("code-378", "label-28");
        try
        {
            item.MoveTo(GlobalOccupancyForecastStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalOccupancyForecastStatus.Revoked, "status");
    }

    public static void GlobalOccupancyForecastRejectsMovesFromExpired()
    {
        var item = GlobalOccupancyForecast.NewGlobalOccupancyForecast("code-378", "label-28");
        item.Status = GlobalOccupancyForecastStatus.Expired;
        Check.Fails(() => item.MoveTo(GlobalOccupancyForecastStatus.Requested), "move");
    }

    public static void RunAll()
    {
        GlobalOccupancyForecastStartsInRequested();
        GlobalOccupancyForecastMovesToRevoked();
        GlobalOccupancyForecastRejectsMovesFromExpired();
    }
}
