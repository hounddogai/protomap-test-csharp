namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorEntry;

using Acme.Workforce.Workplace.Visitor.VisitorEntry;

/// <summary>Checks of visitor entry code, run by the project's runner.</summary>
public static class PendingVisitorAssignmentChecks
{
    public static void PendingVisitorAssignmentStartsInRequested()
    {
        var item = PendingVisitorAssignment.NewPendingVisitorAssignment("code-185", "label-951");
        Check.Equal(item.Status, PendingVisitorAssignmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingVisitorAssignmentMovesToRevoked()
    {
        var item = PendingVisitorAssignment.NewPendingVisitorAssignment("code-185", "label-951");
        try
        {
            item.MoveTo(PendingVisitorAssignmentStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingVisitorAssignmentStatus.Revoked, "status");
    }

    public static void PendingVisitorAssignmentRejectsMovesFromExpired()
    {
        var item = PendingVisitorAssignment.NewPendingVisitorAssignment("code-185", "label-951");
        item.Status = PendingVisitorAssignmentStatus.Expired;
        Check.Fails(() => item.MoveTo(PendingVisitorAssignmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PendingVisitorAssignmentStartsInRequested();
        PendingVisitorAssignmentMovesToRevoked();
        PendingVisitorAssignmentRejectsMovesFromExpired();
    }
}
