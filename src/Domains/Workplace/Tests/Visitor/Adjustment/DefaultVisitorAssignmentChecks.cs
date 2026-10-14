namespace Acme.Workforce.Workplace.Tests.Visitor.Adjustment;

using Acme.Workforce.Workplace.Visitor.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class DefaultVisitorAssignmentChecks
{
    public static void DefaultVisitorAssignmentStartsInDraft()
    {
        var item = DefaultVisitorAssignment.NewDefaultVisitorAssignment("code-51", "label-180");
        Check.Equal(item.Status, DefaultVisitorAssignmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultVisitorAssignmentMovesToSuspended()
    {
        var item = DefaultVisitorAssignment.NewDefaultVisitorAssignment("code-51", "label-180");
        try
        {
            item.MoveTo(DefaultVisitorAssignmentStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultVisitorAssignmentStatus.Suspended, "status");
    }

    public static void DefaultVisitorAssignmentRejectsMovesFromClosed()
    {
        var item = DefaultVisitorAssignment.NewDefaultVisitorAssignment("code-51", "label-180");
        item.Status = DefaultVisitorAssignmentStatus.Closed;
        Check.Fails(() => item.MoveTo(DefaultVisitorAssignmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        DefaultVisitorAssignmentStartsInDraft();
        DefaultVisitorAssignmentMovesToSuspended();
        DefaultVisitorAssignmentRejectsMovesFromClosed();
    }
}
