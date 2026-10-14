namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementSummary;

using Acme.Workforce.Contractors.Engagement.EngagementSummary;

/// <summary>Checks of engagement summary code, run by the project's runner.</summary>
public static class RollingEngagementForecastChecks
{
    public static void RollingEngagementForecastStartsInOpen()
    {
        var item = RollingEngagementForecast.NewRollingEngagementForecast("code-902", "label-83");
        Check.Equal(item.Status, RollingEngagementForecastStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingEngagementForecastMovesToInReview()
    {
        var item = RollingEngagementForecast.NewRollingEngagementForecast("code-902", "label-83");
        try
        {
            item.MoveTo(RollingEngagementForecastStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingEngagementForecastStatus.InReview, "status");
    }

    public static void RollingEngagementForecastRejectsMovesFromArchived()
    {
        var item = RollingEngagementForecast.NewRollingEngagementForecast("code-902", "label-83");
        item.Status = RollingEngagementForecastStatus.Archived;
        Check.Fails(() => item.MoveTo(RollingEngagementForecastStatus.Open), "move");
    }

    public static void RunAll()
    {
        RollingEngagementForecastStartsInOpen();
        RollingEngagementForecastMovesToInReview();
        RollingEngagementForecastRejectsMovesFromArchived();
    }
}
