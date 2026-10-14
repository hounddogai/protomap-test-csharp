namespace Acme.Workforce.Contractors.Tests.Rate.Score;

using Acme.Workforce.Contractors.Rate.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class StrictRateForecastChecks
{
    public static void StrictRateForecastStartsInProposed()
    {
        var item = StrictRateForecast.NewStrictRateForecast("code-924", "title-332");
        Check.Equal(item.Status, StrictRateForecastStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictRateForecastMovesToRejected()
    {
        var item = StrictRateForecast.NewStrictRateForecast("code-924", "title-332");
        try
        {
            item.MoveTo(StrictRateForecastStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictRateForecastStatus.Rejected, "status");
    }

    public static void StrictRateForecastRejectsMovesFromWithdrawn()
    {
        var item = StrictRateForecast.NewStrictRateForecast("code-924", "title-332");
        item.Status = StrictRateForecastStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(StrictRateForecastStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        StrictRateForecastStartsInProposed();
        StrictRateForecastMovesToRejected();
        StrictRateForecastRejectsMovesFromWithdrawn();
    }
}
