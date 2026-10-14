namespace Acme.Workforce.Compensation.Tests.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

/// <summary>Checks of primary pool schedule code, run by the project's runner.</summary>
public static class BonusPoolForecastChecks
{
    public static void BonusPoolForecastStartsInIdle()
    {
        var item = BonusPoolForecast.NewBonusPoolForecast("code-225", "title-62");
        Check.Equal(item.Status, BonusPoolForecastStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolForecastMovesToWarming()
    {
        var item = BonusPoolForecast.NewBonusPoolForecast("code-225", "title-62");
        try
        {
            item.MoveTo(BonusPoolForecastStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolForecastStatus.Warming, "status");
    }

    public static void BonusPoolForecastRejectsMovesFromStopped()
    {
        var item = BonusPoolForecast.NewBonusPoolForecast("code-225", "title-62");
        item.Status = BonusPoolForecastStatus.Stopped;
        Check.Fails(() => item.MoveTo(BonusPoolForecastStatus.Idle), "move");
    }

    public static void RunAll()
    {
        BonusPoolForecastStartsInIdle();
        BonusPoolForecastMovesToWarming();
        BonusPoolForecastRejectsMovesFromStopped();
    }
}
