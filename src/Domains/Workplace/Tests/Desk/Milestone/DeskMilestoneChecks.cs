namespace Acme.Workforce.Workplace.Tests.Desk.Milestone;

using Acme.Workforce.Workplace.Desk.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class DeskMilestoneChecks
{
    public static void DeskMilestoneStartsInProposed()
    {
        var item = DeskMilestone.NewDeskMilestone("code-644", "name-322");
        Check.Equal(item.Status, DeskMilestoneStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskMilestoneMovesToApproved()
    {
        var item = DeskMilestone.NewDeskMilestone("code-644", "name-322");
        try
        {
            item.MoveTo(DeskMilestoneStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskMilestoneStatus.Approved, "status");
    }

    public static void DeskMilestoneRejectsMovesFromWithdrawn()
    {
        var item = DeskMilestone.NewDeskMilestone("code-644", "name-322");
        item.Status = DeskMilestoneStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(DeskMilestoneStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        DeskMilestoneStartsInProposed();
        DeskMilestoneMovesToApproved();
        DeskMilestoneRejectsMovesFromWithdrawn();
    }
}
