namespace Acme.Workforce.Contractors.Tests.Offboarding.Profile;

using Acme.Workforce.Contractors.Offboarding.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class StrictOffboardingForecastChecks
{
    public static void StrictOffboardingForecastStartsInDraft()
    {
        var item = StrictOffboardingForecast.NewStrictOffboardingForecast("code-838", "title-913");
        Check.Equal(item.Status, StrictOffboardingForecastStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictOffboardingForecastMovesToSuspended()
    {
        var item = StrictOffboardingForecast.NewStrictOffboardingForecast("code-838", "title-913");
        try
        {
            item.MoveTo(StrictOffboardingForecastStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictOffboardingForecastStatus.Suspended, "status");
    }

    public static void StrictOffboardingForecastRejectsMovesFromClosed()
    {
        var item = StrictOffboardingForecast.NewStrictOffboardingForecast("code-838", "title-913");
        item.Status = StrictOffboardingForecastStatus.Closed;
        Check.Fails(() => item.MoveTo(StrictOffboardingForecastStatus.Draft), "move");
    }

    public static void RunAll()
    {
        StrictOffboardingForecastStartsInDraft();
        StrictOffboardingForecastMovesToSuspended();
        StrictOffboardingForecastRejectsMovesFromClosed();
    }
}
