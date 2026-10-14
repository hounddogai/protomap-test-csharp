namespace Acme.Workforce.Engagement.Tests.TownHall.Review;

using Acme.Workforce.Engagement.TownHall.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class TownHallForecastChecks
{
    public static void TownHallForecastStartsInRequested()
    {
        var item = TownHallForecast.NewTownHallForecast("code-205", "title-274");
        Check.Equal(item.Status, TownHallForecastStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallForecastMovesToRevoked()
    {
        var item = TownHallForecast.NewTownHallForecast("code-205", "title-274");
        try
        {
            item.MoveTo(TownHallForecastStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallForecastStatus.Revoked, "status");
    }

    public static void TownHallForecastRejectsMovesFromExpired()
    {
        var item = TownHallForecast.NewTownHallForecast("code-205", "title-274");
        item.Status = TownHallForecastStatus.Expired;
        Check.Fails(() => item.MoveTo(TownHallForecastStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TownHallForecastStartsInRequested();
        TownHallForecastMovesToRevoked();
        TownHallForecastRejectsMovesFromExpired();
    }
}
