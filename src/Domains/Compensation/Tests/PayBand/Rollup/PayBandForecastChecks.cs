namespace Acme.Workforce.Compensation.Tests.PayBand.Rollup;

using Acme.Workforce.Compensation.PayBand.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PayBandForecastChecks
{
    public static void PayBandForecastStartsInCreated()
    {
        var item = PayBandForecast.NewPayBandForecast("code-348", "label-555");
        Check.Equal(item.Status, PayBandForecastStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayBandForecastMovesToPublished()
    {
        var item = PayBandForecast.NewPayBandForecast("code-348", "label-555");
        try
        {
            item.MoveTo(PayBandForecastStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayBandForecastStatus.Published, "status");
    }

    public static void PayBandForecastRejectsMovesFromRetired()
    {
        var item = PayBandForecast.NewPayBandForecast("code-348", "label-555");
        item.Status = PayBandForecastStatus.Retired;
        Check.Fails(() => item.MoveTo(PayBandForecastStatus.Created), "move");
    }

    public static void RunAll()
    {
        PayBandForecastStartsInCreated();
        PayBandForecastMovesToPublished();
        PayBandForecastRejectsMovesFromRetired();
    }
}
