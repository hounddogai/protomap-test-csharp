namespace Acme.Workforce.Talent.Tests.Pool.Budget;

using Acme.Workforce.Talent.Pool.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PoolForecastChecks
{
    public static void PoolForecastStartsInCreated()
    {
        var item = PoolForecast.NewPoolForecast("code-8", "name-660");
        Check.Equal(item.Status, PoolForecastStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PoolForecastMovesToPublished()
    {
        var item = PoolForecast.NewPoolForecast("code-8", "name-660");
        try
        {
            item.MoveTo(PoolForecastStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PoolForecastStatus.Published, "status");
    }

    public static void PoolForecastRejectsMovesFromRetired()
    {
        var item = PoolForecast.NewPoolForecast("code-8", "name-660");
        item.Status = PoolForecastStatus.Retired;
        Check.Fails(() => item.MoveTo(PoolForecastStatus.Created), "move");
    }

    public static void RunAll()
    {
        PoolForecastStartsInCreated();
        PoolForecastMovesToPublished();
        PoolForecastRejectsMovesFromRetired();
    }
}
