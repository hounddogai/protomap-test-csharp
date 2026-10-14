namespace Acme.Workforce.Compliance.Tests.Audit.Assignment;

using Acme.Workforce.Compliance.Audit.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class AuditAssignmentChecks
{
    public static void AuditAssignmentStartsInDraft()
    {
        var item = AuditAssignment.NewAuditAssignment("code-83", "name-795");
        Check.Equal(item.Status, AuditAssignmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AuditAssignmentMovesToPending()
    {
        var item = AuditAssignment.NewAuditAssignment("code-83", "name-795");
        try
        {
            item.MoveTo(AuditAssignmentStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AuditAssignmentStatus.Pending, "status");
    }

    public static void AuditAssignmentRejectsMovesFromClosed()
    {
        var item = AuditAssignment.NewAuditAssignment("code-83", "name-795");
        item.Status = AuditAssignmentStatus.Closed;
        Check.Fails(() => item.MoveTo(AuditAssignmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AuditAssignmentStartsInDraft();
        AuditAssignmentMovesToPending();
        AuditAssignmentRejectsMovesFromClosed();
    }
}
