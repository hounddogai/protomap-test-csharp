namespace Acme.Workforce.Workplace.Tests.Visitor.Assignment;

using Acme.Workforce.Workplace.Visitor.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class VisitorMilestoneChecks
{
    public static void VisitorMilestoneStartsInIdle()
    {
        var item = VisitorMilestone.NewVisitorMilestone("code-514", "title-147");
        Check.Equal(item.Status, VisitorMilestoneStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorMilestoneMovesToWarming()
    {
        var item = VisitorMilestone.NewVisitorMilestone("code-514", "title-147");
        try
        {
            item.MoveTo(VisitorMilestoneStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorMilestoneStatus.Warming, "status");
    }

    public static void VisitorMilestoneRejectsMovesFromStopped()
    {
        var item = VisitorMilestone.NewVisitorMilestone("code-514", "title-147");
        item.Status = VisitorMilestoneStatus.Stopped;
        Check.Fails(() => item.MoveTo(VisitorMilestoneStatus.Idle), "move");
    }

    public static void RunAll()
    {
        VisitorMilestoneStartsInIdle();
        VisitorMilestoneMovesToWarming();
        VisitorMilestoneRejectsMovesFromStopped();
    }
}
