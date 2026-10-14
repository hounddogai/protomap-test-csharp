namespace Acme.Workforce.Talent.Tests.CareerPath.PathRule;

using Acme.Workforce.Talent.CareerPath.PathRule;

/// <summary>Checks of path rule code, run by the project's runner.</summary>
public static class TieredCareerPathForecastChecks
{
    public static void TieredCareerPathForecastStartsInCreated()
    {
        var item = TieredCareerPathForecast.NewTieredCareerPathForecast("code-777", "name-110");
        Check.Equal(item.Status, TieredCareerPathForecastStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredCareerPathForecastMovesToPublished()
    {
        var item = TieredCareerPathForecast.NewTieredCareerPathForecast("code-777", "name-110");
        try
        {
            item.MoveTo(TieredCareerPathForecastStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredCareerPathForecastStatus.Published, "status");
    }

    public static void TieredCareerPathForecastRejectsMovesFromRetired()
    {
        var item = TieredCareerPathForecast.NewTieredCareerPathForecast("code-777", "name-110");
        item.Status = TieredCareerPathForecastStatus.Retired;
        Check.Fails(() => item.MoveTo(TieredCareerPathForecastStatus.Created), "move");
    }

    public static void RunAll()
    {
        TieredCareerPathForecastStartsInCreated();
        TieredCareerPathForecastMovesToPublished();
        TieredCareerPathForecastRejectsMovesFromRetired();
    }
}
