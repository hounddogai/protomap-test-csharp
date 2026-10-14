namespace Acme.Workforce.Contractors.Tests.Rate.Budget;

using Acme.Workforce.Contractors.Rate.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class LenientRateForecastChecks
{
    public static void LenientRateForecastStartsInOpen()
    {
        var item = LenientRateForecast.NewLenientRateForecast("code-751", "label-237");
        Check.Equal(item.Status, LenientRateForecastStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientRateForecastMovesToResolved()
    {
        var item = LenientRateForecast.NewLenientRateForecast("code-751", "label-237");
        try
        {
            item.MoveTo(LenientRateForecastStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientRateForecastStatus.Resolved, "status");
    }

    public static void LenientRateForecastRejectsMovesFromArchived()
    {
        var item = LenientRateForecast.NewLenientRateForecast("code-751", "label-237");
        item.Status = LenientRateForecastStatus.Archived;
        Check.Fails(() => item.MoveTo(LenientRateForecastStatus.Open), "move");
    }

    public static void RunAll()
    {
        LenientRateForecastStartsInOpen();
        LenientRateForecastMovesToResolved();
        LenientRateForecastRejectsMovesFromArchived();
    }
}
