namespace Acme.Workforce.Talent.Tests.Pool.Baseline;

using Acme.Workforce.Talent.Pool.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class StrictPoolForecastChecks
{
    public static void StrictPoolForecastStartsInIdle()
    {
        var item = StrictPoolForecast.NewStrictPoolForecast("code-282", "label-743");
        Check.Equal(item.Status, StrictPoolForecastStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictPoolForecastMovesToWarming()
    {
        var item = StrictPoolForecast.NewStrictPoolForecast("code-282", "label-743");
        try
        {
            item.MoveTo(StrictPoolForecastStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictPoolForecastStatus.Warming, "status");
    }

    public static void StrictPoolForecastRejectsMovesFromStopped()
    {
        var item = StrictPoolForecast.NewStrictPoolForecast("code-282", "label-743");
        item.Status = StrictPoolForecastStatus.Stopped;
        Check.Fails(() => item.MoveTo(StrictPoolForecastStatus.Idle), "move");
    }

    public static void RunAll()
    {
        StrictPoolForecastStartsInIdle();
        StrictPoolForecastMovesToWarming();
        StrictPoolForecastRejectsMovesFromStopped();
    }
}
