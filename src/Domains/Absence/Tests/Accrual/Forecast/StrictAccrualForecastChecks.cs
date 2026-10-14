namespace Acme.Workforce.Absence.Tests.Accrual.Forecast;

using Acme.Workforce.Absence.Accrual.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class StrictAccrualForecastChecks
{
    public static void StrictAccrualForecastStartsInProposed()
    {
        var item = StrictAccrualForecast.NewStrictAccrualForecast("code-214", "name-613");
        Check.Equal(item.Status, StrictAccrualForecastStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictAccrualForecastMovesToApproved()
    {
        var item = StrictAccrualForecast.NewStrictAccrualForecast("code-214", "name-613");
        try
        {
            item.MoveTo(StrictAccrualForecastStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictAccrualForecastStatus.Approved, "status");
    }

    public static void StrictAccrualForecastRejectsMovesFromWithdrawn()
    {
        var item = StrictAccrualForecast.NewStrictAccrualForecast("code-214", "name-613");
        item.Status = StrictAccrualForecastStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(StrictAccrualForecastStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        StrictAccrualForecastStartsInProposed();
        StrictAccrualForecastMovesToApproved();
        StrictAccrualForecastRejectsMovesFromWithdrawn();
    }
}
