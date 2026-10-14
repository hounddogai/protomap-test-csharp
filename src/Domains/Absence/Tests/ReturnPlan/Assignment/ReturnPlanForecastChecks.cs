namespace Acme.Workforce.Absence.Tests.ReturnPlan.Assignment;

using Acme.Workforce.Absence.ReturnPlan.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class ReturnPlanForecastChecks
{
    public static void ReturnPlanForecastStartsInCreated()
    {
        var item = ReturnPlanForecast.NewReturnPlanForecast("code-907", "title-653");
        Check.Equal(item.Status, ReturnPlanForecastStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReturnPlanForecastMovesToPublished()
    {
        var item = ReturnPlanForecast.NewReturnPlanForecast("code-907", "title-653");
        try
        {
            item.MoveTo(ReturnPlanForecastStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReturnPlanForecastStatus.Published, "status");
    }

    public static void ReturnPlanForecastRejectsMovesFromRetired()
    {
        var item = ReturnPlanForecast.NewReturnPlanForecast("code-907", "title-653");
        item.Status = ReturnPlanForecastStatus.Retired;
        Check.Fails(() => item.MoveTo(ReturnPlanForecastStatus.Created), "move");
    }

    public static void RunAll()
    {
        ReturnPlanForecastStartsInCreated();
        ReturnPlanForecastMovesToPublished();
        ReturnPlanForecastRejectsMovesFromRetired();
    }
}
