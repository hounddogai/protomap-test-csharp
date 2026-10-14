namespace Acme.Workforce.Payrollops.Tests.RetroPay.Review;

using Acme.Workforce.Payrollops.RetroPay.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class CappedRetroPayForecastChecks
{
    public static void CappedRetroPayForecastStartsInNew()
    {
        var item = CappedRetroPayForecast.NewCappedRetroPayForecast("code-620", "name-378");
        Check.Equal(item.Status, CappedRetroPayForecastStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedRetroPayForecastMovesToDone()
    {
        var item = CappedRetroPayForecast.NewCappedRetroPayForecast("code-620", "name-378");
        try
        {
            item.MoveTo(CappedRetroPayForecastStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedRetroPayForecastStatus.Done, "status");
    }

    public static void CappedRetroPayForecastRejectsMovesFromFailed()
    {
        var item = CappedRetroPayForecast.NewCappedRetroPayForecast("code-620", "name-378");
        item.Status = CappedRetroPayForecastStatus.Failed;
        Check.Fails(() => item.MoveTo(CappedRetroPayForecastStatus.New), "move");
    }

    public static void RunAll()
    {
        CappedRetroPayForecastStartsInNew();
        CappedRetroPayForecastMovesToDone();
        CappedRetroPayForecastRejectsMovesFromFailed();
    }
}
