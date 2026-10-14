namespace Acme.Workforce.Talent.Tests.CareerPath.Forecast;

using Acme.Workforce.Talent.CareerPath.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class FallbackCareerPathForecastChecks
{
    public static void FallbackCareerPathForecastStartsInCreated()
    {
        var item = FallbackCareerPathForecast.NewFallbackCareerPathForecast("code-976", "label-766");
        Check.Equal(item.Status, FallbackCareerPathForecastStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackCareerPathForecastMovesToValidated()
    {
        var item = FallbackCareerPathForecast.NewFallbackCareerPathForecast("code-976", "label-766");
        try
        {
            item.MoveTo(FallbackCareerPathForecastStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackCareerPathForecastStatus.Validated, "status");
    }

    public static void FallbackCareerPathForecastRejectsMovesFromRetired()
    {
        var item = FallbackCareerPathForecast.NewFallbackCareerPathForecast("code-976", "label-766");
        item.Status = FallbackCareerPathForecastStatus.Retired;
        Check.Fails(() => item.MoveTo(FallbackCareerPathForecastStatus.Created), "move");
    }

    public static void RunAll()
    {
        FallbackCareerPathForecastStartsInCreated();
        FallbackCareerPathForecastMovesToValidated();
        FallbackCareerPathForecastRejectsMovesFromRetired();
    }
}
