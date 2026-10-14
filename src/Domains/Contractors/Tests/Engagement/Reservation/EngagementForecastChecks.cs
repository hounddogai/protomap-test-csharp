namespace Acme.Workforce.Contractors.Tests.Engagement.Reservation;

using Acme.Workforce.Contractors.Engagement.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class EngagementForecastChecks
{
    public static void EngagementForecastStartsInNew()
    {
        var item = EngagementForecast.NewEngagementForecast("code-775", "label-414");
        Check.Equal(item.Status, EngagementForecastStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementForecastMovesToRunning()
    {
        var item = EngagementForecast.NewEngagementForecast("code-775", "label-414");
        try
        {
            item.MoveTo(EngagementForecastStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementForecastStatus.Running, "status");
    }

    public static void EngagementForecastRejectsMovesFromFailed()
    {
        var item = EngagementForecast.NewEngagementForecast("code-775", "label-414");
        item.Status = EngagementForecastStatus.Failed;
        Check.Fails(() => item.MoveTo(EngagementForecastStatus.New), "move");
    }

    public static void RunAll()
    {
        EngagementForecastStartsInNew();
        EngagementForecastMovesToRunning();
        EngagementForecastRejectsMovesFromFailed();
    }
}
