namespace Acme.Workforce.Assets.Tests.Assignment.Milestone;

using Acme.Workforce.Assets.Assignment.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class AssignmentMilestoneChecks
{
    public static void AssignmentMilestoneStartsInOpen()
    {
        var item = AssignmentMilestone.NewAssignmentMilestone("code-989", "label-926");
        Check.Equal(item.Status, AssignmentMilestoneStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentMilestoneMovesToInReview()
    {
        var item = AssignmentMilestone.NewAssignmentMilestone("code-989", "label-926");
        try
        {
            item.MoveTo(AssignmentMilestoneStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentMilestoneStatus.InReview, "status");
    }

    public static void AssignmentMilestoneRejectsMovesFromArchived()
    {
        var item = AssignmentMilestone.NewAssignmentMilestone("code-989", "label-926");
        item.Status = AssignmentMilestoneStatus.Archived;
        Check.Fails(() => item.MoveTo(AssignmentMilestoneStatus.Open), "move");
    }

    public static void RunAll()
    {
        AssignmentMilestoneStartsInOpen();
        AssignmentMilestoneMovesToInReview();
        AssignmentMilestoneRejectsMovesFromArchived();
    }
}
