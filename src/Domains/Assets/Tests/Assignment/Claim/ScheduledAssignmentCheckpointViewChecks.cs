namespace Acme.Workforce.Assets.Tests.Assignment.Claim;

using Acme.Workforce.Assets.Assignment.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class ScheduledAssignmentCheckpointViewChecks
{
    public static void ScheduledAssignmentCheckpointViewShowsTheStatusInCapitals()
    {
        var item = ScheduledAssignmentCheckpoint.NewScheduledAssignmentCheckpoint("code-3", "title-733");
        var view = ScheduledAssignmentCheckpointView.ToScheduledAssignmentCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledAssignmentCheckpointView.ToScheduledAssignmentCheckpointViews(new List<ScheduledAssignmentCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledAssignmentCheckpointViewShowsTheStatusInCapitals();
    }
}
