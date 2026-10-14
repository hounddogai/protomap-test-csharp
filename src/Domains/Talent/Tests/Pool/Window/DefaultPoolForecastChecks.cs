namespace Acme.Workforce.Talent.Tests.Pool.Window;

using Acme.Workforce.Talent.Pool.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class DefaultPoolForecastChecks
{
    public static void DefaultPoolForecastStartsInRequested()
    {
        var item = DefaultPoolForecast.NewDefaultPoolForecast("code-50", "label-207");
        Check.Equal(item.Status, DefaultPoolForecastStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultPoolForecastMovesToGranted()
    {
        var item = DefaultPoolForecast.NewDefaultPoolForecast("code-50", "label-207");
        try
        {
            item.MoveTo(DefaultPoolForecastStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultPoolForecastStatus.Granted, "status");
    }

    public static void DefaultPoolForecastRejectsMovesFromExpired()
    {
        var item = DefaultPoolForecast.NewDefaultPoolForecast("code-50", "label-207");
        item.Status = DefaultPoolForecastStatus.Expired;
        Check.Fails(() => item.MoveTo(DefaultPoolForecastStatus.Requested), "move");
    }

    public static void RunAll()
    {
        DefaultPoolForecastStartsInRequested();
        DefaultPoolForecastMovesToGranted();
        DefaultPoolForecastRejectsMovesFromExpired();
    }
}
