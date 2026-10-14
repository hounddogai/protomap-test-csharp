namespace Acme.Workforce.Talent.Tests.Pool.Request;

using Acme.Workforce.Talent.Pool.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class AdaptivePoolForecastChecks
{
    public static void AdaptivePoolForecastStartsInIdle()
    {
        var item = AdaptivePoolForecast.NewAdaptivePoolForecast("code-383", "title-779");
        Check.Equal(item.Status, AdaptivePoolForecastStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptivePoolForecastMovesToStopped()
    {
        var item = AdaptivePoolForecast.NewAdaptivePoolForecast("code-383", "title-779");
        try
        {
            item.MoveTo(AdaptivePoolForecastStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptivePoolForecastStatus.Stopped, "status");
    }

    public static void AdaptivePoolForecastRejectsMovesFromStopped()
    {
        var item = AdaptivePoolForecast.NewAdaptivePoolForecast("code-383", "title-779");
        item.Status = AdaptivePoolForecastStatus.Stopped;
        Check.Fails(() => item.MoveTo(AdaptivePoolForecastStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AdaptivePoolForecastStartsInIdle();
        AdaptivePoolForecastMovesToStopped();
        AdaptivePoolForecastRejectsMovesFromStopped();
    }
}
