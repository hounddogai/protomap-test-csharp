namespace Acme.Workforce.Workplace.Tests.Visitor.Assignment;

using Acme.Workforce.Workplace.Visitor.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class VisitorAssignmentChecks
{
    public static void VisitorAssignmentStartsInIdle()
    {
        var item = VisitorAssignment.NewVisitorAssignment("code-608", "title-504");
        Check.Equal(item.Status, VisitorAssignmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorAssignmentMovesToDraining()
    {
        var item = VisitorAssignment.NewVisitorAssignment("code-608", "title-504");
        try
        {
            item.MoveTo(VisitorAssignmentStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorAssignmentStatus.Draining, "status");
    }

    public static void VisitorAssignmentRejectsMovesFromStopped()
    {
        var item = VisitorAssignment.NewVisitorAssignment("code-608", "title-504");
        item.Status = VisitorAssignmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(VisitorAssignmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        VisitorAssignmentStartsInIdle();
        VisitorAssignmentMovesToDraining();
        VisitorAssignmentRejectsMovesFromStopped();
    }
}
