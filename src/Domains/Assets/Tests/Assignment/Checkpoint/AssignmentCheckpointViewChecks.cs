namespace Acme.Workforce.Assets.Tests.Assignment.Checkpoint;

using Acme.Workforce.Assets.Assignment.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AssignmentCheckpointViewChecks
{
    public static void AssignmentCheckpointViewShowsTheStatusInCapitals()
    {
        var item = AssignmentCheckpoint.NewAssignmentCheckpoint("code-510", "label-98");
        var view = AssignmentCheckpointView.ToAssignmentCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssignmentCheckpointView.ToAssignmentCheckpointViews(new List<AssignmentCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssignmentCheckpointViewShowsTheStatusInCapitals();
    }
}
