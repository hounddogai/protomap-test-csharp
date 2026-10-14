namespace Acme.Workforce.Assets.Tests.Assignment.Milestone;

using Acme.Workforce.Assets.Assignment.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class AssignmentMilestoneViewChecks
{
    public static void AssignmentMilestoneViewShowsTheStatusInCapitals()
    {
        var item = AssignmentMilestone.NewAssignmentMilestone("code-989", "label-926");
        var view = AssignmentMilestoneView.ToAssignmentMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssignmentMilestoneView.ToAssignmentMilestoneViews(new List<AssignmentMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssignmentMilestoneViewShowsTheStatusInCapitals();
    }
}
