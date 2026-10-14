namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementSummary;

using Acme.Workforce.Contractors.Engagement.EngagementSummary;

/// <summary>Checks of engagement summary code, run by the project's runner.</summary>
public static class EngagementCheckpointViewChecks
{
    public static void EngagementCheckpointViewShowsTheStatusInCapitals()
    {
        var item = EngagementCheckpoint.NewEngagementCheckpoint("code-873", "title-860");
        var view = EngagementCheckpointView.ToEngagementCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EngagementCheckpointView.ToEngagementCheckpointViews(new List<EngagementCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EngagementCheckpointViewShowsTheStatusInCapitals();
    }
}
