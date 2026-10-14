namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Forecast;

using Acme.Workforce.Talent.SuccessionPlan.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class SuccessionPlanForecastChecks
{
    public static void SuccessionPlanForecastStartsInCreated()
    {
        var item = SuccessionPlanForecast.NewSuccessionPlanForecast("code-853", "label-788");
        Check.Equal(item.Status, SuccessionPlanForecastStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SuccessionPlanForecastMovesToPublished()
    {
        var item = SuccessionPlanForecast.NewSuccessionPlanForecast("code-853", "label-788");
        try
        {
            item.MoveTo(SuccessionPlanForecastStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SuccessionPlanForecastStatus.Published, "status");
    }

    public static void SuccessionPlanForecastRejectsMovesFromRetired()
    {
        var item = SuccessionPlanForecast.NewSuccessionPlanForecast("code-853", "label-788");
        item.Status = SuccessionPlanForecastStatus.Retired;
        Check.Fails(() => item.MoveTo(SuccessionPlanForecastStatus.Created), "move");
    }

    public static void RunAll()
    {
        SuccessionPlanForecastStartsInCreated();
        SuccessionPlanForecastMovesToPublished();
        SuccessionPlanForecastRejectsMovesFromRetired();
    }
}
