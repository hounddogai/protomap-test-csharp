namespace Acme.Workforce.Contractors.Tests.Rate.RateBatch;

using Acme.Workforce.Contractors.Rate.RateBatch;

/// <summary>Checks of rate batch code, run by the project's runner.</summary>
public static class RateForecastChecks
{
    public static void RateForecastStartsInNew()
    {
        var item = RateForecast.NewRateForecast("code-875", "title-962");
        Check.Equal(item.Status, RateForecastStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RateForecastMovesToQueued()
    {
        var item = RateForecast.NewRateForecast("code-875", "title-962");
        try
        {
            item.MoveTo(RateForecastStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RateForecastStatus.Queued, "status");
    }

    public static void RateForecastRejectsMovesFromFailed()
    {
        var item = RateForecast.NewRateForecast("code-875", "title-962");
        item.Status = RateForecastStatus.Failed;
        Check.Fails(() => item.MoveTo(RateForecastStatus.New), "move");
    }

    public static void RunAll()
    {
        RateForecastStartsInNew();
        RateForecastMovesToQueued();
        RateForecastRejectsMovesFromFailed();
    }
}
