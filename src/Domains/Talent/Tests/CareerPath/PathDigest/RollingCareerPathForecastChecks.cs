namespace Acme.Workforce.Talent.Tests.CareerPath.PathDigest;

using Acme.Workforce.Talent.CareerPath.PathDigest;

/// <summary>Checks of path digest code, run by the project's runner.</summary>
public static class RollingCareerPathForecastChecks
{
    public static void RollingCareerPathForecastStartsInDraft()
    {
        var item = RollingCareerPathForecast.NewRollingCareerPathForecast("code-502", "title-376");
        Check.Equal(item.Status, RollingCareerPathForecastStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingCareerPathForecastMovesToSuspended()
    {
        var item = RollingCareerPathForecast.NewRollingCareerPathForecast("code-502", "title-376");
        try
        {
            item.MoveTo(RollingCareerPathForecastStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingCareerPathForecastStatus.Suspended, "status");
    }

    public static void RollingCareerPathForecastRejectsMovesFromClosed()
    {
        var item = RollingCareerPathForecast.NewRollingCareerPathForecast("code-502", "title-376");
        item.Status = RollingCareerPathForecastStatus.Closed;
        Check.Fails(() => item.MoveTo(RollingCareerPathForecastStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RollingCareerPathForecastStartsInDraft();
        RollingCareerPathForecastMovesToSuspended();
        RollingCareerPathForecastRejectsMovesFromClosed();
    }
}
