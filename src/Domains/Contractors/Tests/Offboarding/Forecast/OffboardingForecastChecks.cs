namespace Acme.Workforce.Contractors.Tests.Offboarding.Forecast;

using Acme.Workforce.Contractors.Offboarding.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class OffboardingForecastChecks
{
    public static void OffboardingForecastStartsInDraft()
    {
        var item = OffboardingForecast.NewOffboardingForecast("code-119", "label-128");
        Check.Equal(item.Status, OffboardingForecastStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OffboardingForecastMovesToClosed()
    {
        var item = OffboardingForecast.NewOffboardingForecast("code-119", "label-128");
        try
        {
            item.MoveTo(OffboardingForecastStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OffboardingForecastStatus.Closed, "status");
    }

    public static void OffboardingForecastRejectsMovesFromClosed()
    {
        var item = OffboardingForecast.NewOffboardingForecast("code-119", "label-128");
        item.Status = OffboardingForecastStatus.Closed;
        Check.Fails(() => item.MoveTo(OffboardingForecastStatus.Draft), "move");
    }

    public static void RunAll()
    {
        OffboardingForecastStartsInDraft();
        OffboardingForecastMovesToClosed();
        OffboardingForecastRejectsMovesFromClosed();
    }
}
