namespace Acme.Workforce.Absence.Tests.Accrual.Slot;

using Acme.Workforce.Absence.Accrual.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class AccrualForecastChecks
{
    public static void AccrualForecastStartsInPlanned()
    {
        var item = AccrualForecast.NewAccrualForecast("code-617", "name-440");
        Check.Equal(item.Status, AccrualForecastStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualForecastMovesToInProgress()
    {
        var item = AccrualForecast.NewAccrualForecast("code-617", "name-440");
        try
        {
            item.MoveTo(AccrualForecastStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualForecastStatus.InProgress, "status");
    }

    public static void AccrualForecastRejectsMovesFromCancelled()
    {
        var item = AccrualForecast.NewAccrualForecast("code-617", "name-440");
        item.Status = AccrualForecastStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AccrualForecastStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AccrualForecastStartsInPlanned();
        AccrualForecastMovesToInProgress();
        AccrualForecastRejectsMovesFromCancelled();
    }
}
