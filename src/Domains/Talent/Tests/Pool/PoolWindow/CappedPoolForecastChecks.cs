namespace Acme.Workforce.Talent.Tests.Pool.PoolWindow;

using Acme.Workforce.Talent.Pool.PoolWindow;

/// <summary>Checks of pool window code, run by the project's runner.</summary>
public static class CappedPoolForecastChecks
{
    public static void CappedPoolForecastStartsInDraft()
    {
        var item = CappedPoolForecast.NewCappedPoolForecast("code-543", "name-991");
        Check.Equal(item.Status, CappedPoolForecastStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedPoolForecastMovesToPending()
    {
        var item = CappedPoolForecast.NewCappedPoolForecast("code-543", "name-991");
        try
        {
            item.MoveTo(CappedPoolForecastStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedPoolForecastStatus.Pending, "status");
    }

    public static void CappedPoolForecastRejectsMovesFromClosed()
    {
        var item = CappedPoolForecast.NewCappedPoolForecast("code-543", "name-991");
        item.Status = CappedPoolForecastStatus.Closed;
        Check.Fails(() => item.MoveTo(CappedPoolForecastStatus.Draft), "move");
    }

    public static void RunAll()
    {
        CappedPoolForecastStartsInDraft();
        CappedPoolForecastMovesToPending();
        CappedPoolForecastRejectsMovesFromClosed();
    }
}
